import {
  Heading,
  VStack,
  Button,
  useDisclosure,
  Input,
  HStack,
  Text,
} from "@chakra-ui/react";
import { useUser } from "contexts/User";
import { useTranslation } from "react-i18next";
import { useSortingAndFiltering } from "helpers/hooks/useSortingAndFiltering";
import { useActivitySourceList } from "api/activitysources";
import { ActivitySourceSummary } from "components/ActivitySourceSummary";
import { useState } from "react";
import { useBackendApi } from "contexts/BackendApi";
import { DeleteModal } from "components/DeleteModal";
import { Link } from "react-router-dom";
import { FaPlus } from "react-icons/fa";

export const UserHome = () => {
  const { user } = useUser();
  const { t } = useTranslation();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [selectedActivitySource, setSelectedActivitySource] = useState();
  const { activitysource } = useBackendApi();
  const { data, mutate } = useActivitySourceList();
  const {
    sorting,
    setSorting,
    onSort: handleSort,
    filter,
    setFilter,
    outputList,
  } = useSortingAndFiltering(data, "displayName", {
    initialSort: {
      key: "displayName",
    },
  });

  const onDeleteSource = async () => {
    await activitysource.delete({ id: selectedActivitySource.id });
    await mutate();
    onClose();
  };
  const onClickDelete = (activitySource) => {
    setSelectedActivitySource(activitySource);
    onOpen();
  };

  return (
    <VStack align="stretch" px={8} w="100%" spacing={4} p={4}>
      <Heading as="h2" size="lg">
        {t("home.heading", { name: user?.fullName })}
      </Heading>
      <VStack w="100%" align="stretch" spacing={4}>
        <Heading as="h3" size="lg">
          Activity Sources
        </Heading>
        <HStack>
          <Button
            as={Link}
            to="/activitysources/new"
            colorScheme="green"
            leftIcon={<FaPlus />}
          >
            New
          </Button>
          <Input
            placeholder="Search Activity Sources"
            onChange={(e) => setFilter(e.target.value)}
          />
        </HStack>
        {outputList &&
          outputList.map((item, index) => (
            <ActivitySourceSummary
              key={index}
              href={`/activitysources/${item.id}`}
              onDelete={() => onClickDelete(item)}
              title={item.displayName}
              sourceURL={item.host}
              collectionId={item.resourceId}
            ></ActivitySourceSummary>
          ))}
      </VStack>
      <DeleteModal
        title={`Delete Activity Source?`}
        body={
          <VStack>
            <Text>Are you sure you want to delete this activity source:</Text>
            <Text fontWeight="bold">{selectedActivitySource?.displayName}</Text>
            <Text>You will not be able to reverse this!</Text>
          </VStack>
        }
        isOpen={isOpen}
        onClose={onClose}
        onDelete={onDeleteSource}
      />
    </VStack>
  );
};
