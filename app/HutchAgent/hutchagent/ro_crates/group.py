import json
from typing import List
from sqlalchemy import and_, or_

from hutchagent.ro_crates.operator import Operator
from hutchagent.ro_crates.rule import Rule
from hutchagent.ro_crates.thing import Thing


class Group(Thing):
    """Python representation of a group based on [ItemList](https://schema.org/ItemList)."""

    def __init__(
        self,
        context: str,
        type_: str,
        name: str,
        number_of_items: int,
        rules: List[Rule],
        rule_operator: Operator,
        **kwargs
    ) -> None:
        super().__init__(context, type_, name)
        self.number_of_items = number_of_items
        self.rules = rules
        self.rule_operator = rule_operator

    def to_dict(self) -> dict:
        """Convert `Group` to `dict`.

        Returns:
            dict: `Group` as a `dict`.
        """
        item_list_element = [self.rule_operator.to_dict()]
        item_list_element.extend(
            [element.to_dict() for element in self.rules]
        )
        return {
            "@context": self.context,
            "@type": self.type_,
            "name": self.name,
            "numberOfItems": self.number_of_items,
            "itemListElement": [self.rule_operator.to_dict()]
            + [element.to_dict() for element in self.rules],
        }

    @classmethod
    def from_dict(cls, dict_: dict):
        """Create a `Group` from RO-Crate JSON.

        Args:
            dict_ (dict): Mapping containing the `Group`'s attributes.

        Returns:
            Self: `Group` object.
        """

        rules = []
        operator = None
        for i in dict_.get("itemListElement", []):
            if i.get("name") == "ruleOperator":
                operator = Operator.from_dict(i)
            else:
                rules.append(Rule.from_dict(i))
        return cls(
            context=dict_.get("@context"),
            type_=dict_.get("@type"),
            name=dict_.get("name"),
            number_of_items=dict_.get("numberOfItems"),
            rules=rules,
            rule_operator=operator,
        )

    @property
    def sql_clause(self):
        if self.rule_operator.value == "AND":
            return and_(*[rule.sql_clause for rule in self.rules])
        return or_(*[rule.sql_clause for rule in self.rules])

    def __str__(self) -> str:
        """`Group` as a JSON string.

        Returns:
            str: JSON string.
        """
        return json.dumps(self.to_dict(), indent=2)
