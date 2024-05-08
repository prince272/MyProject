import React, { useState } from "react";
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  useDisclosure,
} from "@nextui-org/modal";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Select, SelectItem } from "@nextui-org/select";
import axios, { isAxiosError } from "axios";
import toast from "react-hot-toast";

interface FormData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: string;
}

type FormErrors = Record<keyof FormData, string[]> & { [key: string]: string[] };

type RegisterButtonProps = React.ComponentProps<typeof Button>;

const RegisterButton = (props: RegisterButtonProps) => {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [formData, setFormData] = useState<FormData>({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    role: "",
  });
  const [formErrors, setFormErrors] = useState<FormErrors | null | undefined>();

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    setFormErrors((prevState) => {
      if (!prevState) return prevState;
      const { [e.target.name]: deletedError, ...updatedErrors } = prevState;
      return updatedErrors as FormErrors;
    });
  };

  const handleSubmit = async () => {
    try {
      const response = await axios.post(
        `${process.env.SERVER_URL}/users/register`,
        formData,
        { withCredentials: true }
      );

      toast.success("User registered successfully");

      setFormErrors(null);
      onClose();
    } catch (error) {
      setFormErrors(
        (isAxiosError(error) && error.response?.data?.errors) || {}
      );

      toast.error("Failed to register user");
    }
  };

  return (
    <>
      <Button
        {...props}
        onPress={(e) => {
          onOpen();
          props.onPress && props.onPress(e);
        }}
      >Register</Button>
      <Modal isOpen={isOpen} onOpenChange={onOpen} onClose={onClose}>
        <ModalContent>
          <ModalHeader>Register Form</ModalHeader>
          <ModalBody>
            <Input
              name="firstName"
              placeholder="First Name"
              onChange={handleChange}
              isInvalid={!!formErrors?.firstName?.length}
              errorMessage={formErrors?.firstName?.join("\n")}
            />
            <Input
              name="lastName"
              placeholder="Last Name"
              onChange={handleChange}
              isInvalid={!!formErrors?.lastName?.length}
              errorMessage={formErrors?.lastName?.join("\n")}
            />
            <Input
              name="email"
              placeholder="Email"
              onChange={handleChange}
              isInvalid={!!formErrors?.email?.length}
              errorMessage={formErrors?.email?.join("\n")}
            />
            <Input
              name="password"
              placeholder="Password"
              onChange={handleChange}
              isInvalid={!!formErrors?.password?.length}
              errorMessage={formErrors?.password?.join("\n")}
            />
            <Select
              name="role"
              placeholder="Role"
              onChange={handleChange}
              isInvalid={!!formErrors?.role?.length}
              errorMessage={formErrors?.role?.join("\n")}
            >
              <SelectItem key="admin" value="admin">
                Admin
              </SelectItem>
              <SelectItem key="member" value="member">
                Member
              </SelectItem>
            </Select>
          </ModalBody>
          <ModalFooter>
            <Button onClick={handleSubmit}>Submit</Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};

export { RegisterButton };
