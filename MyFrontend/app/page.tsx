"use client";

import { Button } from "@nextui-org/button";
import { RegisterButton } from "./components/register-button";
import { LoginButton } from "./components/login-button";

export default function Page() {
  return (
    <div className="p-9">
      <div className="flex gap-3">
        <RegisterButton color="primary" />
        <LoginButton />
      </div>
    </div>
  );
}
