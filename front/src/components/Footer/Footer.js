import React from 'react';
import './Footer.css';

const Footer = () => {
    return (
        <footer className="footer">
            <div className="footer-top">
                <div className="footer-container">
                    <div className="footer-logo">
                        <img src="/assets/svg/logo.png" alt="CityGuide" className="logo-img" />
                    </div>
                    <nav className="footer-nav">                    
                        <a href="/catalog" className="footer-nav-link">Каталог</a>
                    </nav>

                    <div className="footer-email">
                        <span className="email-text">example@mail.ru</span>
                    </div>
                </div>
            </div>

            <div className="footer-divider"></div>

            <div className="footer-bottom">
                <div className="footer-container">
                    <div className="copyright">
                        © Loga 2025. Все права защищены.
                    </div>
                    <div className="privacy">
                        <a href="/privacy" className="privacy-link">Политика конфиденциальности</a>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;