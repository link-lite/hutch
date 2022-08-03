import pytest
from hutchagent.ro_crates.group import Group
from hutchagent.ro_crates.operator import Operator
from hutchagent.ro_crates.query import Query
from hutchagent.ro_crates.result import Result
from hutchagent.ro_crates.rule import Rule

ACTIVITY_SOURCE_ID_DICT = {
    "@context": "https://schema.org",
    "@type": "PropertyValue",
    "name": "activity_source_id",
    "value": "fake_activity_source_id",
}

JOB_ID_DICT = {
    "@context": "https://schema.org",
    "@type": "PropertyValue",
    "name": "job_id",
    "value": "fake_job_id",
}

GROUP_OPERATOR_DICT = {
    "@context": "https://schema.org",
    "@type": "PropertyVale",
    "name": "groupOperator",
    "value": "AND",
}

RULE_DICT = {
    "@context": "https://schema.org",
    "@type": "QuantitativeValue",
    "value": "8532",
    "additionalProperty": {
        "@context": "https://schema.org",
        "@type": "PropertyVale",
        "name": "operator",
        "value": "=",
    },
}


GROUP_DICT = {
    "@context": "https://schema.org",
    "@type": "ItemList",
    "name": "group",
    "numberOfItems": 3,
    "itemListElement": [
        {
            "@context": "https://schema.org",
            "@type": "PropertyVale",
            "name": "ruleOperator",
            "value": "AND",
        },
        *[RULE_DICT] * 3,
    ],
}


QUERY_DICT = {
    "@context": "https://w3id.org/ro/crate/1.1/context",
    "@graph": [
        ACTIVITY_SOURCE_ID_DICT,
        JOB_ID_DICT,
        GROUP_OPERATOR_DICT,
        GROUP_DICT,
    ],
}


def test_rule():
    rule = Rule.from_dict(RULE_DICT)
    assert rule.to_dict() == RULE_DICT
    assert isinstance(rule.operator, Operator)
    assert str(rule.sql_clause) == "gender_concept_id = :gender_concept_id_1"


def test_group():
    group = Group.from_dict(GROUP_DICT)
    assert len(group.rules) == GROUP_DICT["numberOfItems"]
    for element in group.rules:
        assert isinstance(element, Rule)
    assert isinstance(group.rule_operator, Operator)
    assert group.to_dict() == GROUP_DICT
    assert str(group.sql_clause) == " AND ".join(
        [
            "gender_concept_id = :gender_concept_id_1",
            "gender_concept_id = :gender_concept_id_2",
            "gender_concept_id = :gender_concept_id_3",
        ]
    )


def test_query():
    query = Query.from_dict(QUERY_DICT)
    for g in query.groups:
        assert isinstance(g, Group)
    assert isinstance(query.group_operator, Operator)
    assert query.activity_source_id == ACTIVITY_SOURCE_ID_DICT.get("value")
    assert query.job_id == JOB_ID_DICT.get("value")
    assert query.to_dict() == QUERY_DICT
    assert (
        str(query.to_sql())
        == """SELECT count(*) AS count_1 
FROM (SELECT DISTINCT person.person_id AS person_id 
FROM person JOIN condition_occurrence ON person.person_id = condition_occurrence.person_id JOIN measurement ON person.person_id = measurement.person_id JOIN observation ON person.person_id = observation.person_id 
WHERE gender_concept_id = :gender_concept_id_1 AND gender_concept_id = :gender_concept_id_2 AND gender_concept_id = :gender_concept_id_3) AS anon_1"""
    )


def test_result():
    result = Result(
        activity_source_id="fake_activity_source",
        job_id="fake_job_id",
        status="ok",
        count=100,
    )
    result_dict = {
        "@context": "https://w3id.org/ro/crate/1.1/context",
        "@graph": [
            {
                "@context": "https://schema.org",
                "@type": "PropertyValue",
                "name": "activity_source_id",
                "value": "fake_activity_source",
            },
            {
                "@context": "https://schema.org",
                "@type": "PropertyValue",
                "name": "job_id",
                "value": "fake_job_id",
            },
            {
                "@context": "https://schema.org",
                "@type": "PropertyValue",
                "name": "status",
                "value": "ok",
            },
            {
                "@context": "https://schema.org",
                "@type": "PropertyValue",
                "name": "count",
                "value": 100,
            },
        ],
    }
    assert result.to_dict() == result_dict
