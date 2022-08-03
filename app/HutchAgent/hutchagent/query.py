import datetime as dt
import re
import dotenv
from sqlalchemy import (
    and_,
    column,
    func,
    not_,
    or_,
    select,
    sql,
)
from typing import Any, Union
from hutchagent.db_manager import SyncDBManager
from hutchagent.entities import Concept, ConditionOccurrence, Measurement, Observation, Person, DrugExposure, ProcedureOccurrence
from hutchagent.ro_crates.query import Query

dotenv.load_dotenv()

PERSON_LOOKUPS = {
    "8532": "gender_concept_id",
    "8507": "gender_concept_id",
    "8515": "race_concept_id",
    "8516": "race_concept_id",
    "8527": "race_concept_id",
}


class RQuestQueryRule:
    """Represents an RQuest query rule."""

    def __init__(
        self,
        varname: str = "",
        type: str = "",
        oper: str = "",
        value: str = "",
        time: str = "",
        ext: str = "",
        regex: str = "",
        unit: str = "",
    ) -> None:
        """Constructor for `RQuestQueryRule`.

        Args:
            varname (str, optional): The name of the value column. Defaults to "".
            type (str, optional): The data type of the value. Defaults to "".
            oper (str, optional): The comparison operator for the value. Defaults to "".
            value (str, optional): The value. Defaults to "". Is converted from a string
            to the type specified in `type`.
        """
        self.concept_id = self._parse_concept_id(varname, value)
        self.varname = varname
        self.type = type
        self.oper = oper
        self.value = self._parse_value(value)
        self.column_name = PERSON_LOOKUPS.get(self.concept_id)
        self.time_ = time
        self.ext = ext
        self.regex = regex
        self.unit = unit
        # Set these with `set_table` and `set_column`
        self.table = None
        self.column = None

    def _parse_value(self, value: str) -> Any:
        """Parse string value into correct type.

        Args:
            value (str): The value to be parsed.

        Returns:
            Any: The value with the correct type.
        """
        if self.type == "NUM":
            return self._numeric_value(value)
        else:
            return value

    @property
    def sql_clause(self):
        clause = None
        if self.type == "TEXT" and self.oper == "=":
            clause = self.column == self.concept_id
        elif self.type == "TEXT" and self.oper == "!=":
            clause = self.column != self.concept_id
        elif self.type == "NUM" and self.oper == "=":
            clause = self.column.between(self.value[0], self.value[1])
        elif self.type == "NUM" and self.oper == "!=":
            clause = not_(
                self.column.between(self.value[0], self.value[1])
            )

        # If there is a time clause, combine with main clause.
        if self.time_ != "":
            clause = and_(clause, self._get_time_clause())

        return clause

    def _numeric_value(self, value: str) -> tuple[float, float]:
        lower_bound, upper_bound = value.split("..")
        lower_bound = float(lower_bound)
        upper_bound = float(upper_bound)
        return lower_bound, upper_bound

    def _parse_concept_id(self, id_: str, alt_id: str) -> str:
        """Parses the concept ID from the rule body.

        Args:
            id_ (str): The field to resolve into a concept ID.
            alt_id (str): The alternative value to be used as the concept ID.

        Returns:
            str: The parsed concept ID.
        """
        pattern = re.compile(r"^OMOP=(\d+)$")
        if hit := re.search(pattern, id_):
            return hit.group(1)
        return alt_id

    def _get_time_clause(self):
        """If an RQuest message has a "time" clause, this function is used
        to parse it into an SQL clause.
        """
        date_columns = {
            Person: Person.birth_datetime,
            Measurement: Measurement.measurement_date,
            ConditionOccurrence: ConditionOccurrence.condition_start_date,
            Observation: Observation.observation_date,
            ProcedureOccurrence: ProcedureOccurrence.procedure_date,
            DrugExposure: DrugExposure.drug_exposure_start_date,
        }
        # greater than pattern
        gt_pattern = re.compile(r"^\|(\d+):[a-zA-Z]+:(\w)$")
        # less than pattern
        lt_pattern = re.compile(r"^(\d+)\|:[a-zA-Z]+:(\w)$")

        # If the clause matches the "greater than" pattern
        if hit := re.search(gt_pattern, self.time_):
            timespan = int(hit.group(1))
            time_unit = hit.group(2)
            # older times are smaller, so use `>` for inclusive ("=") seaches
            if time_unit == "Y" and self.oper == "=":
                return date_columns[self.table] < (
                    dt.datetime.now() - dt.timedelta(days=365 * timespan)
                )
            elif time_unit == "M" and self.oper == "=":
                return date_columns[self.table] < (
                    dt.datetime.now() - dt.timedelta(weeks=4.33 * timespan)
                )
            # newer times are larger, so use `<=` for exclusive ("!=") seaches
            elif time_unit == "Y" and self.oper == "!=":
                return date_columns[self.table] >= (
                    dt.datetime.now() - dt.timedelta(days=365 * timespan)
                )
            elif time_unit == "M" and self.oper == "!=":
                return date_columns[self.table] >= (
                    dt.datetime.now() - dt.timedelta(weeks=4.33 * timespan)
                )
            else:
                return None

        # If the clause matches the "less than" pattern
        elif hit := re.search(lt_pattern, self.time_):
            timespan = int(hit.group(1))
            time_unit = hit.group(2)
            # newer times are larger, so use `>` for inclusive ("=") seaches
            if time_unit == "Y" and self.oper == "=":
                return date_columns[self.table] > (
                    dt.datetime.now() - dt.timedelta(days=365 * timespan)
                )
            elif time_unit == "M" and self.oper == "=":
                return date_columns[self.table] > (
                    dt.datetime.now() - dt.timedelta(weeks=4.33 * timespan)
                )
            # older times are smaller, so use `<=` for exclusive ("!=") seaches
            elif time_unit == "Y" and self.oper == "!=":
                return date_columns[self.table] <= (
                    dt.datetime.now() - dt.timedelta(days=365 * timespan)
                )
            elif time_unit == "M" and self.oper == "!=":
                return date_columns[self.table] <= (
                    dt.datetime.now() - dt.timedelta(weeks=4.33 * timespan)
                )
            else:
                return None

        else:
            raise ValueError(
                f"Could not parse the time value ({self.time_}) in the rule."
            )

    def set_table(self, table):
        """Set the table for the rule."""
        self.table = table

    def set_column(self, column):
        """Set the column for the rule"""
        self.column = column


class RQuestQueryGroup:
    """Represents an RQuest query group."""

    def __init__(
        self, rules: list = None, rules_oper: str = "", exclude: bool = False
    ) -> None:
        """Constructor for `RQuestQueryGroup`.

        Args:
            rules (list, optional): A list of `RQuestQueryRule`s. Defaults to None.
            rules_oper (str, optional): The operand for comparing the rules. Defaults to "".
            exclude (bool, optional): Exclude results match the group rules. Defaults to False.
        """
        self.rules = (
            [RQuestQueryRule(**r) for r in rules] if rules is not None else list()
        )
        self.rules_oper = rules_oper
        self.exclude = exclude

    @property
    def columns(self):
        return [column(rule.column_name) for rule in self.rules]

    @property
    def sql_clause(self):
        if self.rules_oper == "AND":
            return and_(*[rule.sql_clause for rule in self.rules])
        else:
            return or_(*[rule.sql_clause for rule in self.rules])


class RQuestQueryCohort:
    """Represents an RQuest query cohort."""

    def __init__(self, groups: list = None, groups_oper: str = "") -> None:
        """Constructor for `RQuestQueryCohort`.

        Args:
            groups (list, optional): A list of `RQuestQueryGroup`s. Defaults to None.
            groups_oper (str, optional): The operand for comparing the groups. Defaults to "".
        """
        self.groups = (
            [RQuestQueryGroup(**g) for g in groups] if groups is not None else list()
        )
        self.groups_oper = groups_oper

    @property
    def sql_clause(self):
        if self.groups_oper == "AND":
            return and_(*[group.sql_clause for group in self.groups])
        else:
            return or_(*[group.sql_clause for group in self.groups])


class RQuestQuery:
    """Represents an RQuest query"""

    def __init__(
        self,
        job_id: str = "",
        activity_source_id: str = "",
        cohort: dict = None,  # mutable types shouldn't used as defaults
        **kwargs,  # ignored args
    ) -> None:
        """Construction for `RQuestQuery`.

        Args:
            job_id (str, optional): The job ID of the query. Defaults to "".
            activity_source_id (str, optional): The activity source ID of the query. Defaults to "".
            cohort (dict, optional): The cohort of groups. Defaults to None.
        """
        self.cohort = cohort if cohort is not None else {}  # turn None to empty dict
        self.cohort = RQuestQueryCohort(**cohort)
        self.job_id = job_id
        self.activity_source_id = activity_source_id

    def to_sql(self):
        columns = set()
        for group in self.cohort.groups:
            for col in group.columns:
                columns.add(col)
        # Make columns appear in ascending order by name for tests.
        columns = sorted(columns, key=lambda x: x.name)

        stmt = (
            select(Person.person_id)
            .join(
                ConditionOccurrence,
                Person.person_id == ConditionOccurrence.person_id,
            )
            .join(
                Measurement,
                Person.person_id == Measurement.person_id,
            )
            .join(
                Observation,
                Person.person_id == Observation.person_id,
            )
            .where(self.cohort.sql_clause)
            .distinct()
            .subquery()
        )
        return select(func.count()).select_from(stmt)


class BaseQueryBuilder:

    domain_table_map = {
        "Drug": DrugExposure.person_id,
        "Ethnicity": Person.person_id,
        "Gender": Person.person_id,
        "Measurement": Measurement.person_id,
        "Observation": Observation.person_id,
        "Procedure": ProcedureOccurrence.person_id,
        "Race": Person.person_id,
    }
    domain_column_map = {
        "Drug": DrugExposure.drug_concept_id,
        "Ethnicity": Person.ethnicity_concept_id,
        "Gender": Person.gender_concept_id,
        "Measurement": Measurement.measurement_concept_id,
        "Observation": Observation.observation_concept_id,
        "Procedure": ProcedureOccurrence.procedure_concept_id,
        "Race": Person.race_concept_id,
    }
    subqueries = list()

    def __init__(self, db_manager: SyncDBManager, query: Union[RQuestQuery, Query]) -> None:
        self.db_manager = db_manager
        self.query = query

    def set_tables_and_columns(self) -> None:
        raise NotImplementedError

    def build_subqueries(self) -> None:
        raise NotImplementedError

    def build_sql(self) -> sql.selectable.Select:
        raise NotImplementedError


class RQuestQueryBuilder(BaseQueryBuilder):

    def set_tables_and_columns(self) -> None:
        """Set the tables and columns for the rules in the query."""
        pass

    def build_subqueries(self) -> None:
        """Build the subqueries for the main query."""
        for group in self.query.cohort.groups:
            for rule in group.rules:
                # Text rules testing for inclusion
                if rule.type == "TEXT" and rule.oper == "=":
                    self.subqueries.append(
                        select(Person.person_id)
                        .join(
                            ProcedureOccurrence,
                            Person.person_id == ProcedureOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            ConditionOccurrence,
                            Person.person_id == ConditionOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            Observation,
                            Person.person_id == Observation.person_id,
                            isouter=True,
                        )
                        .join(
                            DrugExposure,
                            Person.person_id == DrugExposure.person_id,
                            isouter=True,
                        )
                        .where(
                            or_(
                                Person.ethnicity_concept_id == rule.concept_id,
                                Person.gender_concept_id == rule.concept_id,
                                Person.race_concept_id == rule.concept_id,
                                ProcedureOccurrence.procedure_concept_id == rule.concept_id,
                                ConditionOccurrence.condition_concept_id == rule.concept_id,
                                Observation.observation_concept_id == rule.concept_id,
                                DrugExposure.drug_concept_id == rule.concept_id,
                            )
                        )
                        .distinct()
                        .subquery()
                    )
                # Text rules testing for exclusion
                elif rule.type == "TEXT" and rule.oper == "!=":
                    self.subqueries.append(
                        select(Person.person_id)
                        .join(
                            ProcedureOccurrence,
                            Person.person_id == ProcedureOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            ConditionOccurrence,
                            Person.person_id == ConditionOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            Observation,
                            Person.person_id == Observation.person_id,
                            isouter=True,
                        )
                        .join(
                            DrugExposure,
                            Person.person_id == DrugExposure.person_id,
                            isouter=True,
                        )
                        .where(
                            or_(
                                Person.ethnicity_concept_id != rule.concept_id,
                                Person.gender_concept_id != rule.concept_id,
                                Person.race_concept_id != rule.concept_id,
                                ProcedureOccurrence.procedure_concept_id != rule.concept_id,
                                ConditionOccurrence.condition_concept_id != rule.concept_id,
                                Observation.observation_concept_id != rule.concept_id,
                                DrugExposure.drug_concept_id != rule.concept_id,
                            )
                        )
                        .distinct()
                        .subquery()
                    )
                else:
                    # numeric rule
                    self.subqueries.append(
                        select(Measurement.person_id)
                        .where(
                            and_(
                                Measurement.measurement_concept_id == rule.concept_id,
                                Measurement.value_as_number.between(*rule.value)
                            )
                        )
                        .distinct()
                        .subquery()
                    )

    def build_sql(self) -> sql.selectable.Select:
        """Build and return the final SQL that can be used to query the database."""
        stmnt = select(func.count()).select_from(self.subqueries[0])
        for sq in self.subqueries[1:]:
            stmnt = stmnt.join(sq, isouter=True)
        self.subqueries.clear()
        return stmnt


class ROCratesQueryBuilder(BaseQueryBuilder):

    def set_tables_and_columns(self) -> None:
        """Set the tables and columns for the rules in the query."""
        pass

    def build_subqueries(self) -> None:
        """Build the subqueries for the main query."""
        for group in self.query.groups:
            for rule in group.rules:
                # Text rules testing for inclusion
                if rule.value is not None and rule.operator == "=":
                    self.subqueries.append(
                        select(Person.person_id)
                        .join(
                            ProcedureOccurrence,
                            Person.person_id == ProcedureOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            ConditionOccurrence,
                            Person.person_id == ConditionOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            Observation,
                            Person.person_id == Observation.person_id,
                            isouter=True,
                        )
                        .join(
                            DrugExposure,
                            Person.person_id == DrugExposure.person_id,
                            isouter=True,
                        )
                        .where(
                            or_(
                                Person.ethnicity_concept_id == rule.name,
                                Person.gender_concept_id == rule.name,
                                Person.race_concept_id == rule.name,
                                ProcedureOccurrence.procedure_concept_id == rule.name,
                                ConditionOccurrence.condition_concept_id == rule.name,
                                Observation.observation_concept_id == rule.name,
                                DrugExposure.drug_concept_id == rule.name,
                            )
                        )
                        .distinct()
                        .subquery()
                    )
                # Text rules testing for exclusion
                elif rule.value is not None and rule.operator == "!=":
                    self.subqueries.append(
                        select(Person.person_id)
                        .join(
                            ProcedureOccurrence,
                            Person.person_id == ProcedureOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            ConditionOccurrence,
                            Person.person_id == ConditionOccurrence.person_id,
                            isouter=True,
                        )
                        .join(
                            Observation,
                            Person.person_id == Observation.person_id,
                            isouter=True,
                        )
                        .join(
                            DrugExposure,
                            Person.person_id == DrugExposure.person_id,
                            isouter=True,
                        )
                        .where(
                            or_(
                                Person.ethnicity_concept_id != rule.name,
                                Person.gender_concept_id != rule.name,
                                Person.race_concept_id != rule.name,
                                ProcedureOccurrence.procedure_concept_id != rule.name,
                                ConditionOccurrence.condition_concept_id != rule.name,
                                Observation.observation_concept_id != rule.name,
                                DrugExposure.drug_concept_id != rule.name,
                            )
                        )
                        .distinct()
                        .subquery()
                    )
                else:
                    # numeric rule
                    self.subqueries.append(
                        select(Measurement.person_id)
                        .where(
                            and_(
                                Measurement.measurement_concept_id == rule.name,
                                Measurement.value_as_number.between(rule.min_value, rule.max_value)
                            )
                        )
                        .distinct()
                        .subquery()
                    )

    def build_sql(self) -> sql.selectable.Select:
        """Build and return the final SQL that can be used to query the database."""
        stmnt = select(func.count()).select_from(self.subqueries[0])
        for sq in self.subqueries[1:]:
            stmnt = stmnt.join(sq, isouter=True)
        self.subqueries.clear()
        return stmnt
