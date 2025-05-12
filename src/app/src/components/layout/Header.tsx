import "./Header.css";

type HeaderProps = {
  logoSrc: string;
};

export default function Header({ logoSrc }: HeaderProps) {
  return (
    <img
      src={logoSrc}
      alt="logo"
      className="app-header"
    />
  );
}
