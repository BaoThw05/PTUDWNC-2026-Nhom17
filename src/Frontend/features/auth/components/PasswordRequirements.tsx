const RULES = [
  { label: "Ít nhất 8 ký tự", test: (value: string) => value.length >= 8 },
  { label: "Có chữ hoa", test: (value: string) => /[A-Z]/.test(value) },
  { label: "Có chữ thường", test: (value: string) => /[a-z]/.test(value) },
  { label: "Có chữ số", test: (value: string) => /[0-9]/.test(value) },
  { label: "Có ký tự đặc biệt", test: (value: string) => /[^a-zA-Z0-9]/.test(value) },
];

export function PasswordRequirements({ value }: { value: string }) {
  return (
    <ul className="-mt-2 grid grid-cols-1 gap-x-4 gap-y-1 text-xs sm:grid-cols-2" aria-label="Yêu cầu mật khẩu">
      {RULES.map(({ label, test }) => {
        const met = test(value);
        return (
          <li
            key={label}
            className={met ? "text-emerald-700 dark:text-emerald-400" : "text-zinc-500"}
          >
            <span aria-hidden>{met ? "✓" : "○"}</span> {label}
            <span className="sr-only">{met ? " — đã đạt" : " — chưa đạt"}</span>
          </li>
        );
      })}
    </ul>
  );
}
