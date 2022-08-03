import {
  Button,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalFooter,
  ModalBody,
  ModalCloseButton,
  VStack,
  Alert,
  AlertIcon,
  useDisclosure,
  Table,
  Thead,
  Tbody,
  Text,
  Tr,
  Th,
  Td,
  TableCaption,
} from "@chakra-ui/react";

import React, { useState } from "react";
import { Form, Formik, useField } from "formik";
import { FaArrowRight } from "react-icons/fa";
import { FormikInput } from "./forms/FormikInput";
import { FormikSelect } from "./forms/FormikSelect";
import { LowNumberSuppressionParameters } from "./LowNumberSuppressionParameters";
import { validationSchema } from "pages/ResultsModifier/validation";
import { capitaliseObjectKeys } from "helpers/data-structures";
import { objectStringsToNull } from "helpers/data-structures";
import { objectsAreEqual } from "helpers/data-structures";
import { useModifierTypeList } from "api/resultsmodifiers";
import { useActivitySourceList } from "api/activitysources";

export const ConfirmationModal = ({
  isOpen,
  onClose,
  initialData,
  newData,
}) => {
  if (!initialData) return null;

  const getParam = (k, v) => {
    switch (k) {
      case "Type":
        return v.id;
      case "ActivitySource":
        return v.displayName;
      default:
        return v;
    }
  };

  // convert the objects to arrays to be compared and displayed
  const initialDataArray = Object.entries(capitaliseObjectKeys(initialData));
  const newDataArray = Object.entries(newData);

  const parametersDiff = () => {
    // returns empty array if parameters are the same
    // otherwise returns array of objects specifying the key, previous, and new values

    // get the previous and new parameter objects
    const previousParameters = objectStringsToNull(
      capitaliseObjectKeys(capitaliseObjectKeys(initialData).Parameters)
    );
    const newParameters = objectStringsToNull(newData.Parameters);

    // if parameters are equal then return empty array
    if (objectsAreEqual(previousParameters, newParameters)) return [];

    // get all the individual keys from both objects, then return the difference for each of those keys
    const keys = [
      ...new Set([
        ...Object.keys(previousParameters),
        ...Object.keys(newParameters),
      ]),
    ];

    const diff = keys.map((key) => ({
      key: key,
      prev: previousParameters[key] ?? "null",
      new: newParameters[key] ?? "null",
    }));

    return diff;
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          {initialData ? "Update Results Modifier" : "Create Results Modifier"}
        </ModalHeader>
        <ModalCloseButton />
        <Form noValidate>
          <ModalBody>Are you sure you want to do this?</ModalBody>
          <Table variant="simple">
            <TableCaption>Changes made</TableCaption>
            <Thead>
              <Tr>
                <Th>Parameter</Th>
                <Th>Previous</Th>
                <Th>New</Th>
              </Tr>
            </Thead>
            <Tbody>
              {newDataArray
                .filter((item) => item[0] != "Parameters")
                .map((item) => {
                  const initialItem = initialDataArray.find(
                    (el) => el[0] == item[0]
                  );
                  if (objectsAreEqual(initialItem, item)) return null;
                  return (
                    <Tr>
                      <Td>{item[0]}</Td>
                      <Td>
                        <Text color={"red.300"}>
                          {initialItem
                            ? getParam(initialItem[0], initialItem[1])
                            : "undefined"}
                        </Text>
                      </Td>
                      <Td>
                        <Text as="b" color={"green.300"}>
                          {getParam(item[0], item[1])}
                        </Text>
                      </Td>
                    </Tr>
                  );
                })}
              {parametersDiff().length > 0 && (
                <>
                  <Tr>
                    <Td colSpan={3}>
                      <Text as="b">Parameters</Text>
                    </Td>
                  </Tr>
                  {parametersDiff().map((item) => (
                    <Tr>
                      <Td>{item.key}</Td>
                      <Td>
                        <Text color={"red.300"}>{item.prev}</Text>
                      </Td>
                      <Td>
                        <Text as="b" color={"green.300"}>
                          {item.new}
                        </Text>
                      </Td>
                    </Tr>
                  ))}
                </>
              )}
            </Tbody>
          </Table>
          <ModalFooter>
            <Button variant="ghost" mr={3} onClick={onClose}>
              Cancel
            </Button>
            <Button colorScheme="blue" type="submit">
              Confirm
            </Button>
          </ModalFooter>
        </Form>
      </ModalContent>
    </Modal>
  );
};

export const ConfigureResultsModifierModal = ({
  initialData,
  isOpen,
  onClose,
  action,
  mutate,
}) => {
  const {
    isOpen: isConfirmOpen,
    onOpen: onConfirmOpen,
    onClose: onConfirmClose,
  } = useDisclosure();
  const [feedback, setFeedback] = useState();
  const { data: typeOptions } = useModifierTypeList();
  const { data: activitySourceOptions } = useActivitySourceList();
  const onCloseHandler = () => {
    onClose();
    setFeedback(null);
  };
  const handleSubmit = async (values, actions) => {
    try {
      // convert all empty strings to null
      const payload = objectStringsToNull(values);
      // post to the api
      await action({
        values: {
          ...payload,
          ActivitySourceId: payload.ActivitySource,
        },
        id: initialData ? initialData.id : undefined,
      }).json();
      mutate();
      onConfirmClose();
      onCloseHandler();
    } catch (e) {
      console.error(e);
      onConfirmClose();
      setFeedback("Something went wrong!");
      window.scrollTo(0, 0);
    }
    actions.setSubmitting(false);
  };

  return (
    <Modal isOpen={isOpen} onClose={onCloseHandler}>
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          {initialData ? "Update Results Modifier" : "Create Results Modifier"}
        </ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <Formik
            onSubmit={handleSubmit}
            initialValues={
              initialData
                ? {
                  Order: initialData.order,
                  Type: initialData.type.id,
                  // capitalise the object keys in the parameters object
                  Parameters: capitaliseObjectKeys(initialData.parameters),
                  ActivitySource: initialData.activitySource.id,
                }
                : {
                  Order: "0",
                  Type: typeOptions[0].id,
                  Parameters: {},
                  ActivitySource: activitySourceOptions[0].id,
                }
            }
            validationSchema={validationSchema()}
          >
            {({ isSubmitting, values }) => (
              <Form noValidate>
                <VStack align="stretch">
                  {feedback && (
                    <Alert status="error">
                      <AlertIcon />
                      {feedback}
                    </Alert>
                  )}
                  <FormikInput label="Order" name={"Order"} type="number" />
                  <FormikSelect
                    label="Type"
                    name={"Type"}
                    type="Type"
                    options={typeOptions.map((item) => ({
                      value: item.id,
                      label: item.id,
                    }))}
                  />
                  <FormikSelect
                    label="Activity Source"
                    name={"ActivitySource"}
                    type="ActivitySource"
                    options={activitySourceOptions.map((item) => ({
                      value: item.id,
                      label: item.displayName,
                    }))}
                  />
                  <LowNumberSuppressionParameters type={values.Type} />
                  <Button
                    w="full"
                    leftIcon={<FaArrowRight />}
                    colorScheme="blue"
                    type={initialData ? undefined : "submit"}
                    onClick={initialData ? onConfirmOpen : undefined}
                    disabled={isSubmitting}
                    isLoading={isSubmitting}
                  >
                    {initialData
                      ? "Update Results Modifier"
                      : "Create Results Modifier"}
                  </Button>
                  <ConfirmationModal
                    isOpen={isConfirmOpen}
                    onClose={onConfirmClose}
                    initialData={initialData}
                    newData={{
                      ...values,
                      ActivitySource: activitySourceOptions.find(
                        (item) => item.id == values.ActivitySource
                      ),
                      Type: typeOptions.find((item) => item.id == values.Type),
                    }}
                  />
                </VStack>
              </Form>
            )}
          </Formik>
        </ModalBody>
      </ModalContent>
    </Modal>
  );
};
