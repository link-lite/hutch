import {
  Heading,
  HStack,
  Text,
  LinkBox,
  LinkOverlay,
  VStack,
  Box,
  Button,
  Flex,
  Badge,
} from "@chakra-ui/react";
import { FaTrash } from "react-icons/fa";
import { Link } from "react-router-dom";

export const ActivitySourceSummary = ({
  title,
  href,
  sourceURL,
  collectionId,
  children,
  onDelete,
  ...p
}) => (
  <LinkBox>
    <VStack
      bg="gray.100"
      borderColor="gray.300"
      borderWidth={2}
      borderRadius={5}
      h="100%"
      p={4}
      align="stretch"
      _hover={{
        borderColor: "blue.500",
        bg: "gray.50",
      }}
      {...p}
    >
      <HStack>
        <Flex w="full">
          <Heading as="h3" size="md" _hoverGroup={{ color: "blue.500" }}>
            <LinkOverlay w="100%" as={Link} to={`${href}`}>
              {title}
            </LinkOverlay>
          </Heading>
          <Button
            colorScheme="red"
            ml={"auto"}
            onClick={onDelete}
            leftIcon={<FaTrash />}
          >
            Delete
          </Button>
        </Flex>
      </HStack>
      <HStack>
        {sourceURL && (
          <Badge colorScheme="cyan">Source Host: {sourceURL}</Badge>
        )}
        {collectionId && (
          <Badge colourScheme="turquoise">Resource ID: {collectionId}</Badge>
        )}
      </HStack>
    </VStack>
  </LinkBox>
);
