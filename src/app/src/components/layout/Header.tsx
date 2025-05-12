import React from "react";
import "./Header.css";

type HeaderProps = {
  logoSrc: string;
};

const Header: React.FC<HeaderProps> = ({ logoSrc }) => {
  return (
    <img
      src={logoSrc}
      alt="logo"
      className="app-header"
    />
  );
};

export default Header;
