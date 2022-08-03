import pytest
import hutchagent.query as query
import hutchagent.entities as entities


def test_create_rquest_query():
    request_dict = {
        "job_id": "d21599d5-2514-4f63-9183-62ddc47f8e0d",
        "cohort": {
            "groups_oper": "OR",
            "groups": [
                {
                    "rules_oper": "AND",
                    "rules": [
                        {
                            "type": "TEXT",
                            "varname": "OMOP",
                            "oper": "=",
                            "value": "8532",
                            "time": "|6:TIME:M",
                            "ext": "",
                            "unit": "",
                            "regex": "",
                        },
                    ],
                    "exclude": False,
                }
            ],
        },
        "activity_source_id": 1,
    }
    rquest_query = query.RQuestQuery(**request_dict)
    # Assert this created an instance of `RQuestQueryCohort`
    assert isinstance(rquest_query.cohort, query.RQuestQueryCohort)
    # Assert query groups is a list of `RQuestQueryGroup`
    assert isinstance(rquest_query.cohort.groups, list)
    assert isinstance(rquest_query.cohort.groups[0], query.RQuestQueryGroup)
    # Assert query rules is a list of `RQuestQueryRule`
    assert isinstance(rquest_query.cohort.groups[0].rules, list)
    assert isinstance(rquest_query.cohort.groups[0].rules[0], query.RQuestQueryRule)


def test_text_rule_sql_clause():
    eq_rule = {
        "varname": "OMOP",
        "type": "TEXT",
        "oper": "=",
        "value": "8527",
    }
    rule_obj = query.RQuestQueryRule(**eq_rule)
    assert rule_obj.concept_id == "8527"
    rule_obj.set_column(entities.Person.race_concept_id)
    assert str(rule_obj.sql_clause) == "person.race_concept_id = :race_concept_id_1"
    ne_rule = eq_rule.copy()
    ne_rule.update(oper="!=")
    rule_obj = query.RQuestQueryRule(**ne_rule)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert str(rule_obj.sql_clause) == "person.race_concept_id != :race_concept_id_1"


def test_numeric_rule_sql_clause():
    eq_rule = {
        "varname": "OMOP=8527",
        "type": "NUM",
        "oper": "=",
        "value": "10..20",
    }
    rule_obj = query.RQuestQueryRule(**eq_rule)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert rule_obj.concept_id == "8527"
    assert (
        str(rule_obj.sql_clause)
        == "person.race_concept_id BETWEEN :race_concept_id_1 AND :race_concept_id_2"
    )
    ne_rule = eq_rule.copy()
    ne_rule.update(oper="!=")
    rule_obj = query.RQuestQueryRule(**ne_rule)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert (
        str(rule_obj.sql_clause)
        == "person.race_concept_id NOT BETWEEN :race_concept_id_1 AND :race_concept_id_2"
    )


def test_time_rule_sql_clause():
    eq_rule = {
        "varname": "OMOP",
        "type": "TEXT",
        "oper": "=",
        "value": "8527",
        "time": "18|:AGE:Y",
    }
    rule_obj = query.RQuestQueryRule(**eq_rule)
    rule_obj.set_table(entities.Person)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert rule_obj.concept_id == "8527"
    assert (
        str(rule_obj.sql_clause)
        == "person.race_concept_id = :race_concept_id_1 AND person.birth_datetime > :birth_datetime_1"
    )
    ne_rule = eq_rule.copy()
    ne_rule.update(oper="!=")
    rule_obj = query.RQuestQueryRule(**ne_rule)
    rule_obj.set_table(entities.Person)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert (
        str(rule_obj.sql_clause)
        == "person.race_concept_id != :race_concept_id_1 AND person.birth_datetime <= :birth_datetime_1"
    )
    ne_rule.update(time="|18:AGE:Y")
    rule_obj = query.RQuestQueryRule(**ne_rule)
    rule_obj.set_table(entities.Person)
    rule_obj.set_column(entities.Person.race_concept_id)
    assert (
        str(rule_obj.sql_clause)
        == "person.race_concept_id != :race_concept_id_1 AND person.birth_datetime >= :birth_datetime_1"
    )
    ne_rule.update(time="|18|:AGE:Y")
    rule_obj = query.RQuestQueryRule(**ne_rule)
    rule_obj.set_table(entities.Person)
    rule_obj.set_column(entities.Person.race_concept_id)
    with pytest.raises(ValueError):
        print(rule_obj.sql_clause)


def test_group_sql_clause():
    and_rule = {
        "rules": [
            {
                "varname": "OMOP",
                "type": "TEXT",
                "oper": "=",
                "value": "8527",
            },
        ],
        "rules_oper": "OR",
    }
    group = query.RQuestQueryGroup(**and_rule)
    group.rules[0].set_table(entities.Person)
    group.rules[0].set_column(entities.Person.race_concept_id)
    # Assert single rule in group builds correctly
    assert str(group.sql_clause) == "person.race_concept_id = :race_concept_id_1"
    and_rule["rules"].append(
        {
            "varname": "OMOP",
            "type": "TEXT",
            "oper": "=",
            "value": "8532",
        },
    )
    group = query.RQuestQueryGroup(**and_rule)
    group.rules[0].set_table(entities.Person)
    group.rules[0].set_column(entities.Person.race_concept_id)
    group.rules[1].set_table(entities.Person)
    group.rules[1].set_column(entities.Person.gender_concept_id)
    # Assert multiple rules in group build correctly
    assert (
        str(group.sql_clause)
        == "person.race_concept_id = :race_concept_id_1 OR person.gender_concept_id = :gender_concept_id_1"
    )


def test_cohort_sql_clause():
    and_group = {
        "groups": [
            {
                "rules": [
                    {
                        "varname": "OMOP",
                        "type": "TEXT",
                        "oper": "=",
                        "value": "8527",
                    }
                ],
                "rules_oper": "OR",
            },
        ],
        "groups_oper": "AND",
    }
    cohort = query.RQuestQueryCohort(**and_group)
    cohort.groups[0].rules[0].set_table(entities.Person)
    cohort.groups[0].rules[0].set_column(entities.Person.race_concept_id)
    # Assert single rule in single group builds correctly
    assert str(cohort.sql_clause) == "person.race_concept_id = :race_concept_id_1"
    and_group["groups"][0]["rules"].append(
        {
            "varname": "OMOP",
            "type": "TEXT",
            "oper": "=",
            "value": "8532",
        }
    )
    cohort = query.RQuestQueryCohort(**and_group)
    cohort.groups[0].rules[0].set_table(entities.Person)
    cohort.groups[0].rules[0].set_column(entities.Person.race_concept_id)
    cohort.groups[0].rules[1].set_table(entities.Person)
    cohort.groups[0].rules[1].set_column(entities.Person.gender_concept_id)
    # Assert multiple rules in single group build correctly
    assert (
        str(cohort.sql_clause)
        == "person.race_concept_id = :race_concept_id_1 OR person.gender_concept_id = :gender_concept_id_1"
    )
    and_group["groups"].append(
        {
            "rules": [
                {
                    "varname": "OMOP",
                    "type": "TEXT",
                    "oper": "=",
                    "value": "8532",
                }
            ],
            "rules_oper": "OR",
        }
    )
    # Assert multiple rules in multiple groups build correctly
    cohort = query.RQuestQueryCohort(**and_group)
    cohort.groups[0].rules[0].set_table(entities.Person)
    cohort.groups[0].rules[0].set_column(entities.Person.race_concept_id)
    cohort.groups[0].rules[1].set_table(entities.Person)
    cohort.groups[0].rules[1].set_column(entities.Person.gender_concept_id)
    cohort.groups[1].rules[0].set_table(entities.Person)
    cohort.groups[1].rules[0].set_column(entities.Person.gender_concept_id)
    assert (
        str(cohort.sql_clause)
        == "(person.race_concept_id = :race_concept_id_1 OR person.gender_concept_id = :gender_concept_id_1) AND person.gender_concept_id = :gender_concept_id_2"
    )


def test_query_to_sql():
    request_dict = {
        "owner": "user1",
        "cohort": {
            "groups": [
                {
                    "rules": [
                        {
                            "varname": "OMOP",
                            "type": "TEXT",
                            "oper": "=",
                            "value": "8527",
                        }
                    ],
                    "rules_oper": "OR",
                },
            ],
            "groups_oper": "AND",
        },
    }
    test_query = query.RQuestQuery(**request_dict)
    test_query_sql = test_query.to_sql()
    assert (
        str(test_query_sql)
        == """SELECT count(*) AS count_1 
FROM (SELECT DISTINCT person.person_id AS person_id 
FROM person JOIN condition_occurrence ON person.person_id = condition_occurrence.person_id JOIN measurement ON person.person_id = measurement.person_id JOIN observation ON person.person_id = observation.person_id 
WHERE race_concept_id = :race_concept_id_1) AS anon_1"""
    )
    request_dict["cohort"]["groups"][0]["rules"].append(
        {
            "varname": "OMOP",
            "type": "TEXT",
            "oper": "=",
            "value": "8532",
        }
    )
    test_query = query.RQuestQuery(**request_dict)
    test_query_sql = test_query.to_sql()
    assert (
        str(test_query_sql)
        == """SELECT count(*) AS count_1 
FROM (SELECT DISTINCT person.person_id AS person_id 
FROM person JOIN condition_occurrence ON person.person_id = condition_occurrence.person_id JOIN measurement ON person.person_id = measurement.person_id JOIN observation ON person.person_id = observation.person_id 
WHERE gender_concept_id = :gender_concept_id_1 OR race_concept_id = :race_concept_id_1) AS anon_1"""
    )
    request_dict["cohort"]["groups"].append(
        {
            "rules": [
                {
                    "varname": "OMOP",
                    "type": "TEXT",
                    "oper": "=",
                    "value": "8532",
                }
            ],
            "rules_oper": "OR",
        }
    )
    test_query = query.RQuestQuery(**request_dict)
    test_query_sql = test_query.to_sql()
    assert (
        str(test_query_sql)
        == """SELECT count(*) AS count_1 
FROM (SELECT DISTINCT person.person_id AS person_id 
FROM person JOIN condition_occurrence ON person.person_id = condition_occurrence.person_id JOIN measurement ON person.person_id = measurement.person_id JOIN observation ON person.person_id = observation.person_id 
WHERE (gender_concept_id = :gender_concept_id_1 OR race_concept_id = :race_concept_id_1) AND gender_concept_id = :gender_concept_id_2) AS anon_1"""
    )
