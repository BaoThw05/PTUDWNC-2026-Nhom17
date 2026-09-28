import { z } from "zod";

// Giống quy tắc ở backend (NFR-SEC-001, FR-AUTH-007) để báo lỗi ngay trên form.
const email = z.string().trim().min(1, "Vui lòng nhập email.").pipe(z.email("Email không hợp lệ."));

const fullName = z
  .string()
  .trim()
  .min(2, "Họ tên cần từ 2 ký tự.")
  .max(100, "Họ tên tối đa 100 ký tự.");

const userName = z
  .string()
  .trim()
  .min(3, "Tên đăng nhập cần từ 3 ký tự.")
  .max(50, "Tên đăng nhập tối đa 50 ký tự.")
  .regex(/^[a-zA-Z0-9_.]+$/, "Tên đăng nhập chỉ gồm chữ, số, '_' hoặc '.'.");

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
    fullName,
    email,
    userName,
    password: strongPassword,
    confirmPassword: z.string(),
  })
  .refine((values) => values.password === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  });

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Vui lòng nhập mật khẩu hiện tại.").max(128),
    newPassword: strongPassword,
    confirmPassword: z.string(),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  })
  .refine((values) => values.newPassword !== values.currentPassword, {
    path: ["newPassword"],
    message: "Mật khẩu mới phải khác mật khẩu hiện tại.",
  });

export const profileSchema = z.object({ fullName });

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
export type ChangePasswordValues = z.infer<typeof changePasswordSchema>;
export type ProfileValues = z.infer<typeof profileSchema>;
