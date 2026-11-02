import { Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';
import Header from './components/Header/Header';
import Footer from './components/Footer/Footer';

import CatalogPage from './pages/CatalogPage/CatalogPage';
import FavoritesPage from './pages/FavoritesPage/FavoritesPage';
import ExcursionPage from './pages/ExcursionPage/ExcursionPage';
import CartPage from './pages/CartPage/CartPage';
import LoginPage from './pages/LoginPage/LoginPage';
import RegisterPage from './pages/RegisterPage/RegisterPage';

import UserProfile from './pages/ProfilePage/UserProfile';
import AdminPanel from './pages/ProfilePage/AdminPanel';

import { AuthProvider } from './auth/AuthContext';
import { FavoritesProvider } from './context/FavoritesContext';
import { CartProvider } from './context/CartContext';
import ProtectedRoute from './auth/ProtectedRoute';

function App() {
  const [favoriteIds, setFavoriteIds] = useState([]);

  const toggleFavorite = (id) => {
    setFavoriteIds((prev) =>
      prev.includes(id)
        ? prev.filter((item) => item !== id)
        : [...prev, id]
    );
  };

  return (
    <AuthProvider>
      <FavoritesProvider>
        <CartProvider>
      <div className="App">
        <Header />

        <main className="main-content">
          <Routes>
            <Route path="/" element={<Navigate to="/catalog" />} />

            <Route
              path="/catalog"
              element={
                <CatalogPage
                  favoriteIds={favoriteIds}
                  onToggleFavorite={toggleFavorite}
                />
              }
            />

            <Route
              path="/favorites"
              element={
                <FavoritesPage
                  favoriteIds={favoriteIds}
                  onToggleFavorite={toggleFavorite}
                />
              }
            />

            <Route
              path="/tour/:id"
              element={
                <ExcursionPage
                  favoriteIds={favoriteIds}
                  onToggleFavorite={toggleFavorite}
                />
              }
            />

            <Route path="/cart" element={<CartPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* 👤 ТОЛЬКО USER */}
            <Route
              path="/profile"
              element={
                <ProtectedRoute roles={['User']}>
                  <UserProfile />
                </ProtectedRoute>
              }
            />

            {/* 🛠 ТОЛЬКО ADMIN */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute roles={['Admin']}>
                  <AdminPanel />
                </ProtectedRoute>
              }
            />
          </Routes>
        </main>

        <Footer />
      </div>
        </CartProvider>
      </FavoritesProvider>
    </AuthProvider>
  );
}

export default App;
