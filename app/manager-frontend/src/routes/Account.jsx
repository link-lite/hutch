import { Confirm } from "pages/account/Confirm";
import { Login } from "pages/account/Login";
import { Register } from "pages/account/Register";
import { RequestPasswordReset } from "pages/account/RequestPasswordReset";
import { ResendConfirm } from "pages/account/ResendConfirm";
import { ResendPasswordReset } from "pages/account/ResendPasswordReset";
import { ResetPassword } from "pages/account/ResetPassword";
import { NotFound } from "pages/error/NotFound";
import { Route, Routes } from "react-router-dom";

export const Account = () => (
  <Routes>
    <Route path="login" element={<Login />} />
    <Route path="register" element={<Register />} />
    <Route path="confirm" element={<Confirm />} />
    <Route path="confirm/resend" element={<ResendConfirm />} />
    <Route path="password/reset" element={<RequestPasswordReset />} />
    <Route path="password/resend" element={<ResendPasswordReset />} />
    <Route path="password" element={<ResetPassword />} />

    <Route path="*" element={<NotFound />} />
  </Routes>
);
