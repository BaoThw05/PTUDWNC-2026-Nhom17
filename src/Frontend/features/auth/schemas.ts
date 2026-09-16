import { z } from "zod";

// Giống quy tắc ở backend (NFR-SEC-001, FR-AUTH-007) để báo lỗi ngay trên form.
const email = z.string().trim().min(1, "Vui lòng nhập email.").pipe(z.email("Email không hợp lệ."));

const displayName = z
  .string()
  .trim()
  .min(2, "Tên hiển thị cần từ 2 ký tự.")
  .max(100, "Tên hiển thị tối đa 100 ký tự.");

const strongPassword = z
  .string()
  .min(8, "Mật khẩu cần ít nhất 8 ký tự.")
  .max(128, "Mật khẩu tối đa 128 ký tự.")
  .regex(/[A-Z]/, "Mật khẩu cần có chữ hoa.")
  .regex(/[a-z]/, "Mật khẩu cần có chữ thường.")
  .regex(/[0-9]/, "Mật khẩu cần có chữ số.")
  .regex(/[^a-zA-Z0-9]/, "Mật khẩu cần có ký tự đặc biệt.");

export const loginSchema = z.object({
  email,
  password: z.string().min(1, "Vui lòng nhập mật khẩu.").max(128),
});

export const registerSchema = z
  .object({
    displayName,
    email,
    password: strongPassword,
    confirmPassword: z.string(),
  })
  .refine((values) => values.password === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  });

export const profileSchema = z.object({ displayName });

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
export type ProfileValues = z.infer<typeof profileSchema>;
