import { useAuth } from '../auth/AuthContext';
import { useNavigate, useLocation } from 'react-router-dom';

export const useAuthAction = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  return (action) => {
    if (!user) {
      navigate('/login', {
        state: { from: location.pathname }
      });
      return;
    }

    action();
  };
};
