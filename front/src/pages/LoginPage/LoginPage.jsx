import React, { useState } from 'react';
import './LoginPage.css';
import { Link, useNavigate } from 'react-router-dom';
import { login as loginApi } from '../../auth/authService';
import { useAuth } from '../../auth/AuthContext';

const LoginPage = () => {
  const [form, setForm] = useState({
    email: '',
    password: ''
  });

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const data = await loginApi(form); 
      login(data);                       

      if (data.user.role === 'Admin') {
        navigate('/admin');
      } else {
        navigate('/profile');
      }
    } catch (error) {
      alert('Неверный email или пароль');
    }
  };

  return (
    <div className="auth-page">
      <h1 className="auth-title">Авторизация</h1>

      <Link to="/register" className="register-link">
        <p className="auth-subtitle">
          Нет аккаунта? Зарегистрируйтесь сейчас.
        </p>
      </Link>

      <form className="auth-form" onSubmit={handleSubmit}>
        <input
          type="email"
          name="email"
          placeholder="Examplemail@mail.ru"
          value={form.email}
          onChange={handleChange}
          required
        />

        <input
          type="password"
          name="password"
          placeholder="Пароль"
          value={form.password}
          onChange={handleChange}
          required
        />

        <button type="submit" className="auth-btn">
          Войти
        </button>
      </form>
    </div>
  );
};

export default LoginPage;
