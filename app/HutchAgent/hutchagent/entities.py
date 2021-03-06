from sqlalchemy import (
    BigInteger,
    Column,
    Date,
    ForeignKey,
    Integer,
    Numeric,
    String,
    DateTime,
)
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()


class Person(Base):
    __tablename__ = "person"
    person_id = Column(Integer, primary_key=True)
    gender_concept_id = Column(Integer, nullable=False)
    year_of_birth = Column(Integer, nullable=False)
    month_of_birth = Column(Integer, nullable=True)
    day_of_birth = Column(Integer, nullable=True)
    birth_datetime = Column(DateTime, nullable=True)
    race_concept_id = Column(Integer, nullable=False)
    ethnicity_concept_id = Column(Integer, nullable=False)
    location_id = Column(Integer, nullable=True)
    provider_id = Column(Integer, nullable=True)
    care_site_id = Column(Integer, nullable=True)
    person_source_value = Column(String(50), nullable=True)
    gender_source_value = Column(String(50), nullable=True)
    gender_source_concept_id = Column(Integer, nullable=True)
    race_source_value = Column(String(50), nullable=True)
    race_source_concept_id = Column(Integer, nullable=True)
    ethnicity_source_value = Column(String(50), nullable=True)
    ethnicity_source_concept_id = Column(Integer, nullable=True)


class Measurement(Base):
    __tablename__ = "measurement"
    measurement_id = Column(Integer, primary_key=True)
    person_id = Column(Integer, ForeignKey("person.person_id"), nullable=False)
    measurement_concept_id = Column(Integer, nullable=False)
    measurement_date = Column(Date, nullable=False)
    measurement_datetime = Column(DateTime, nullable=True)
    measurement_time = Column(String(10), nullable=True)
    measurement_type_concept_id = Column(Integer, nullable=False)
    operator_concept_id = Column(Integer, nullable=True)
    value_as_number = Column(Numeric, nullable=True)
    value_as_concept_id = Column(Integer, nullable=True)
    unit_concept_id = Column(Integer, nullable=True)
    range_low = Column(Numeric, nullable=True)
    range_high = Column(Numeric, nullable=True)
    provider_id = Column(Integer, nullable=True)
    visit_occurrence_id = Column(Integer, nullable=True)
    visit_detail_id = Column(Integer, nullable=True)
    measurement_source_value = Column(String(50), nullable=True)
    measurement_source_concept_id = Column(Integer, nullable=True)
    unit_source_value = Column(String(50), nullable=True)
    unit_source_concept_id = Column(Integer, nullable=True)
    value_source_value = Column(String(50), nullable=True)
    measurement_event_id = Column(BigInteger, nullable=True)
    meas_event_field_concept_id = Column(Integer, nullable=True)


class ConditionOccurrence(Base):
    __tablename__ = "condition_occurrence"
    condition_occurrence_id = Column(Integer, primary_key=True)
    person_id = Column(Integer, ForeignKey("person.person_id"), nullable=False)
    condition_concept_id = Column(Integer, nullable=False)
    condition_start_date = Column(Date, nullable=False)
    condition_start_datetime = Column(DateTime, nullable=True)
    condition_end_date = Column(Date, nullable=True)
    condition_end_datetime = Column(DateTime, nullable=True)
    condition_type_concept_id = Column(Integer, nullable=False)
    condition_status_concept_id = Column(Integer, nullable=True)
    stop_reason = Column(String(20), nullable=True)
    provider_id = Column(Integer, nullable=True)
    visit_occurrence_id = Column(Integer, nullable=True)
    visit_detail_id = Column(Integer, nullable=True)
    condition_source_value = Column(String(50), nullable=True)
    condition_source_concept_id = Column(Integer, nullable=True)
    condition_status_source_value = Column(String(50), nullable=True)


class Observation(Base):
    __tablename__ = "observation"
    observation_id = Column(Integer, primary_key=True)
    person_id = Column(Integer, ForeignKey("person.person_id"), nullable=False)
    observation_concept_id = Column(Integer, nullable=False)
    observation_date = Column(Date, nullable=False)
    observation_datetime = Column(DateTime, nullable=True)
    observation_type_concept_id = Column(Integer, nullable=False)
    value_as_number = Column(Numeric, nullable=True)
    value_as_string = Column(String(60), nullable=True)
    value_as_concept_id = Column(Integer, nullable=True)
    qualifier_concept_id = Column(Integer, nullable=True)
    unit_concept_id = Column(Integer, nullable=True)
    provider_id = Column(Integer, nullable=True)
    visit_occurrence_id = Column(Integer, nullable=True)
    visit_detail_id = Column(Integer, nullable=True)
    observation_source_value = Column(String(50), nullable=True)
    observation_source_concept_id = Column(Integer, nullable=True)
    unit_source_value = Column(String(50), nullable=True)
    qualifier_source_value = Column(String(50), nullable=True)
    value_source_value = Column(String(50), nullable=True)
    observation_event_id = Column(BigInteger, nullable=True)
    obs_event_field_concept_id = Column(Integer, nullable=True)
