import {
  Heading,
  VStack,
  Link,
  Button,
  useDisclosure,
  Input,
} from "@chakra-ui/react";

import { useResultsModifierList } from "api/resultsmodifiers";
import { useState } from "react";
import { useBackendApi } from "contexts/BackendApi";
import { DeleteModal } from "components/DeleteModal";
import { ConfigureResultsModifierModal } from "components/ConfigureResultsModifierModal";
import { ResultsModifier } from "components/ResultsModifier";

export const ResultsModifiersList = () => {
  const {
    isOpen: isDeleteOpen,
    onOpen: onDeleteOpen,
    onClose: onDeleteClose,
  } = useDisclosure();
  const {
    isOpen: isUpdateOpen,
    onOpen: onUpdateOpen,
    onClose: onUpdateClose,
  } = useDisclosure();
  const [selected, setSelected] = useState();
  const { resultsmodifier } = useBackendApi();
  const { data, mutate } = useResultsModifierList();

  const onDelete = async () => {
    await resultsmodifier.delete({ id: selected.id });
    await mutate();
    setSelected(undefined);
    onDeleteClose();
  };
  const onClickDelete = (item) => {
    setSelected(item);
    onDeleteOpen();
  };
  const closeDelete = () => {
    onDeleteClose();
    setSelected(undefined);
  };
  const closeUpdate = () => {
    onUpdateClose();
    setSelected(undefined);
  };
  const onClickUpdate = (item) => {
    setSelected(item);
    onUpdateOpen();
  };

  return (
    <VStack align="stretch" px={8} w="100%" spacing={4} p={4}>
      <VStack w="100%" align="stretch">
        <Heading as="h3" size="lg">
          Results modifiers
        </Heading>
        <Button onClick={onUpdateOpen}>Add</Button>
        {data &&
          data.map((item, index) => (
            <ResultsModifier
              key={index}
              onDelete={() => onClickDelete(item)}
              onUpdate={() => onClickUpdate(item)}
              item={item}
            ></ResultsModifier>
          ))}
      </VStack>
      <DeleteModal
        title={`Delete Results Modifier ${selected ? selected.id : ""}`}
        body="Are you sure you want to delete this results modifier? You will not be able to reverse this"
        isOpen={isDeleteOpen}
        onClose={closeDelete}
        id={selected ? selected.id : undefined}
        onDelete={onDelete}
      />
      <ConfigureResultsModifierModal
        isOpen={isUpdateOpen}
        onClose={closeUpdate}
        action={selected ? resultsmodifier.update : resultsmodifier.create}
        initialData={selected}
        mutate={mutate}
      />
    </VStack>
  );
};
