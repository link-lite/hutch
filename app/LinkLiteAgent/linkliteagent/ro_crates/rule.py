import json
from typing import Any, Union

from linkliteagent.ro_crates.operator import Operator
from linkliteagent.ro_crates.thing import Thing


class Rule(Thing):
    """Python representation of an rule based on [QuantitativeValue](https://schema.org/QuantitativeValue)."""

    def __init__(
        self,
        context: str,
        type_: str,
        operator: Operator,
        name: str = "",
        value: Any = None,
        min_value: Union[int, float, None] = None,
        max_value: Union[int, float, None] = None,
        **kwargs
    ) -> None:
        super().__init__(context, type_, name)
        self.operator = operator
        self.value = value
        self.min_value = min_value
        self.max_value = max_value

    def to_dict(self) -> dict:
        """Convert `Rule` to `dict`.

        Returns:
            dict: `Rule` as a `dict`.
        """
        dict_ = super().to_dict()
        dict_.update(additionalProperty=self.operator.to_dict())
        if self.value is not None:
            dict_.update(value=self.value)
        elif (self.min_value is not None) and (self.max_value is not None):
            dict_.update(minValue=self.min_value, maxValue=self.max_value)
        return dict_

    @classmethod
    def from_dict(cls, dict_: dict):
        """Create a `Rule` from RO-Crate JSON.

        Args:
            dict_ (dict): Mapping containing the `Rule`'s attributes.

        Returns:
            Self: `Rule` object.
        """
        return cls(
            context=dict_.get("@context"),
            type_=dict_.get("@type"),
            value=dict_.get("value"),
            min_value=dict_.get("minValue"),
            max_value=dict_.get("maxValue"),
            operator=Operator.from_dict(dict_.get("additionalProper"))
        )

    def __str__(self) -> str:
        """`Rule` as a JSON string.

        Returns:
            str: JSON string.
        """
        return json.dumps(self.to_dict(), indent=2)
