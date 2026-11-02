import React, { useState } from 'react';
import './RegisterPage.css';
import { useNavigate } from 'react-router-dom';
import { register } from '../../auth/authService';

const RegisterPage = () => {
  const navigate = useNavigate();

  // локальное состояние формы (UI)
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    email: '',
    password: ''
  });

  // универсальный обработчик инпутов
  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value
    });
  };

  // 🔑 ГЛАВНАЯ ЛОГИКА РЕГИСТРАЦИИ
  const handleSubmit = async (e) => {
    e.preventDefault();

    // payload строго под backend (RegisterRequest.cs)
    const payload = {
      firstName: form.firstName,
      lastName: form.lastName,
      phoneNumber: form.phoneNumber,
      email: form.email,
      password: form.password
    };

    try {
      await register(payload);
      navigate('/login'); // после успешной регистрации — на логин
    } catch (error) {
      console.error(error);
      alert('Ошибка регистрации');
    }
  };

  return (
    <div className="auth-page">
      <h1 className="auth-title">Регистрация</h1>

      <form className="auth-form" onSubmit={handleSubmit}>
        <input
          name="firstName"
          placeholder="Имя"
          value={form.firstName}
          onChange={handleChange}
          required
        />

        <input
          name="lastName"
          placeholder="Фамилия"
          value={form.lastName}
          onChange={handleChange}
        />

        <input
          type="password"
          name="password"
          placeholder="Пароль"
          value={form.password}
          onChange={handleChange}
          required
        />

        <input
          name="phoneNumber"
          placeholder="Телефон"
          value={form.phoneNumber}
          onChange={handleChange}
        />

        <input
          type="email"
          name="email"
          placeholder="Examplemail@mail.ru"
          value={form.email}
          onChange={handleChange}
          required
        />

        <button type="submit" className="auth-btn">
          Зарегистрироваться
        </button>

        <p className="auth-policy">
          Нажимая кнопку «Подать заявку», вы соглашаетесь с политикой
          конфиденциальности и даёте согласие на обработку персональных данных.
        </p>
      </form>
    </div>
  );
};

export default RegisterPage;
