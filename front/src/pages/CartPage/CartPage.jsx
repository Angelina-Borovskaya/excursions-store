import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { removeFromCart, checkoutCart } from '../../services/cartService';
import { useCart } from '../../context/CartContext';
import './CartPage.css';

const API_ORIGIN = 'http://localhost:5066';

const CartPage = () => {
  const { items, loading, reloadCart } = useCart();
  const [isCheckingOut, setIsCheckingOut] = useState(false);

  const handleRemove = async (id) => {
    await removeFromCart(id);
    await reloadCart();
  };

  const handleCheckout = async () => {
    try {
      setIsCheckingOut(true);
      await checkoutCart();

      alert('Спасибо за покупку! 🎉');

      await reloadCart(); // корзина станет пустой
    } catch (e) {
      console.error('Ошибка оформления заказа', e);
      alert('Ошибка при оформлении заказа');
    } finally {
      setIsCheckingOut(false);
    }
  };

  const totalPrice = items.reduce(
    (sum, item) => sum + item.quantity * item.tour.price,
    0
  );

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="cart-page">
      <div className="cart-container">
        <h1 className="cart-title">Корзина</h1>

        {items.length === 0 ? (
          <div className="cart-empty">
            <h2>Корзина пуста</h2>
            <Link to="/catalog">Перейти в каталог</Link>
          </div>
        ) : (
          <>
            <div className="cart-list">
              {items.map(item => (
                <div key={item.id} className="cart-item">
                  <img
                    src={
                      item.tour.imageFileName
                        ? `${API_ORIGIN}/images/tours/${item.tour.imageFileName}`
                        : '/assets/images/placeholder.jpg'
                    }
                    alt={item.tour.title}
                    className="cart-item__image"
                    onError={(e) => {
                      e.currentTarget.onerror = null;
                      e.currentTarget.src = '/assets/images/placeholder.jpg';
                    }}
                  />

                  <div>
                    <h3>{item.tour.title}</h3>
                    <p>
                      {item.tour.city?.country?.name}, {item.tour.city?.name}
                    </p>
                    <p>Кол-во: {item.quantity}</p>

                    <button onClick={() => handleRemove(item.id)}>
                      Удалить
                    </button>
                  </div>

                  <div>
                    {(item.quantity * item.tour.price).toLocaleString()} ₽
                  </div>
                </div>
              ))}
            </div>

            <div className="cart-summary">
              Итого: <strong>{totalPrice.toLocaleString()} ₽</strong>
            </div>

            {/* 🔥 КНОПКА ПОКУПКИ */}
            <div className="cart-checkout">
              <button
                className="cart-checkout__btn"
                onClick={handleCheckout}
                disabled={isCheckingOut}
              >
                {isCheckingOut ? 'Оформляем...' : 'Купить'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default CartPage;
