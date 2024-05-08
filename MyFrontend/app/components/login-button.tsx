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
import axios, { isAxiosError } from "axios";
import toast from "react-hot-toast";

interface User {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[]; // Assuming roles is an array of strings
}


interface FormData {
  email: string;
  password: string;
}

type FormErrors = Record<keyof FormData, string[]> & { [key: string]: string[] };

type LoginButtonProps = React.ComponentProps<typeof Button>;

const LoginButton = (props: LoginButtonProps) => {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [formData, setFormData] = useState<FormData>({
    email: "",
    password: "",
  });
  const [formErrors, setFormErrors] = useState<FormErrors | null | undefined>();
  const [user, setUser] = useState<User | null>(null);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>
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
      await axios.post(
        `${process.env.SERVER_URL}/users/login`,
        formData,
        { withCredentials: true }
      );

      const response = await axios.get<User>(`${process.env.SERVER_URL}/users/me`, { withCredentials: true });
      setUser(response.data);
      setFormErrors(null);

      toast.success("Logged in successfully");
      onClose();
    } catch (error) {
      setFormErrors(
        (isAxiosError(error) && error.response?.data?.errors) || {}
      );

      toast.error("Failed to login");
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
      >Login</Button>

      <Modal isOpen={isOpen} onOpenChange={onOpen} onClose={onClose}>
        <ModalContent>
          <ModalHeader>Login Form</ModalHeader>
          <ModalBody>
            <Input
              name="email"
              placeholder="Email"
              onChange={handleChange}
              isInvalid={!!formErrors?.email?.length}
              errorMessage={formErrors?.email?.join("\n")}
            />
            <Input
              name="password"
              type="password"
              placeholder="Password"
              onChange={handleChange}
              isInvalid={!!formErrors?.password?.length}
              errorMessage={formErrors?.password?.join("\n")}
            />
          </ModalBody>
          <ModalFooter>
            <Button onClick={handleSubmit}>Submit</Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};

export { LoginButton };
