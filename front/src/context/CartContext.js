import React, { createContext, useContext, useEffect, useState } from 'react';
import {
  getCart,
  addToCart,
  removeFromCart,
} from '../services/cartService';

const CartContext = createContext(null);

export const CartProvider = ({ children }) => {
  const [items, setItems] = useState([]);
  const [cartIds, setCartIds] = useState(new Set());
  const [loading, setLoading] = useState(true);

  // 🔹 загрузка корзины с бэка
  const loadCart = async () => {
    try {
      setLoading(true);
      const data = await getCart();

      setItems(data);

      // 🔥 ВАЖНО: формируем Set из tour.id
      setCartIds(new Set(data.map(item => item.tour.id)));
    } catch (e) {
      console.error('Ошибка загрузки корзины', e);
      setItems([]);
      setCartIds(new Set());
    } finally {
      setLoading(false);
    }
  };

  // 🔹 добавить / убрать из корзины
  const toggleCart = async (tourId) => {
    try {
      if (cartIds.has(tourId)) {
        // находим cartItem по tourId
        const item = items.find(i => i.tour.id === tourId);
        if (item) {
          await removeFromCart(item.id);
        }
      } else {
        await addToCart(tourId, 1);
      }

      // 🔥 ВСЕГДА перезагружаем корзину
      await loadCart();
    } catch (e) {
      console.error('Ошибка изменения корзины', e);
    }
  };

  // 🔹 проверка: есть ли тур в корзине
  const isInCart = (tourId) => cartIds.has(tourId);

  // 🔹 загрузка при старте (если авторизован)
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      loadCart();
    } else {
      setLoading(false);
    }
  }, []);

  return (
    <CartContext.Provider
      value={{
        items,
        cartIds,
        loading,
        toggleCart,
        isInCart,
        reloadCart: loadCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const ctx = useContext(CartContext);
  if (!ctx) {
    throw new Error('useCart must be used within CartProvider');
  }
  return ctx;
};
