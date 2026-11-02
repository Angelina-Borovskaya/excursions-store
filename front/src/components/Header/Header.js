import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import './Header.css';

const Header = () => {
  const { user } = useAuth();

  const profileLink = !user
    ? '/login'
    : user.role === 'Admin'
    ? '/admin'
    : '/profile';

  return (
    <header className="header">
      <div className="header-container">
        <div className="logo">
          <img src="/assets/svg/logo.png" alt="CityGuide" className="logo-img" />
        </div>

        <nav className="nav">
          <Link to="/catalog" className="nav-link">Каталог</Link>
        </nav>

        <div className="fav">
          <Link to="/favorites">
            <img src="/assets/icons/like.svg" alt="Избранное" className="cart-icon"/>
          </Link>
        </div>

        <div className="cart">
          <Link to="/cart">
            <img src="/assets/svg/cart.svg" alt="Корзина" className="cart-icon"/>
          </Link>
        </div>

        <div className="nav-link-profile">
          <Link to={profileLink}>
            <img src="/assets/svg/profile.svg" alt="Профиль" className="cart-icon" />
          </Link>
        </div>
      </div>
    </header>
  );
};

export default Header;
