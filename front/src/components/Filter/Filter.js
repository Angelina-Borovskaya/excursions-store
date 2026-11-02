import React, { useEffect, useRef, useState } from 'react';
import './Filter.css';

const createInitialState = (filters) => {
  const state = {};
  Object.keys(filters).forEach((key) => {
    state[key] = new Set();
  });
  return state;
};

const Filter = ({ filters, onChange }) => {
  const [selected, setSelected] = useState(() =>
    createInitialState(filters)
  );

  const isFirstRender = useRef(true);

  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
      return; 
    }

    onChange(selected);
  }, [selected, onChange]);
  const toggleValue = (category, value) => {
    const updated = new Set(selected[category]);
    updated.has(value) ? updated.delete(value) : updated.add(value);

    setSelected({
      ...selected,
      [category]: updated,
    });
  };

  const resetFilters = () => {
    const cleared = createInitialState(filters);
    setSelected(cleared);
    onChange(cleared);
  };

  return (
    <aside className="filter-sidebar">
      <div className="filter-header">
        <span className="filter-header-title">Фильтр</span>
        <button className="filter-reset" onClick={resetFilters}>
          Сбросить
        </button>
      </div>

      {Object.entries(filters).map(([category, values]) => (
        <div key={category} className="filter-block">
          <div className="filter-title">{category}</div>

          <div className={`filter-box ${category === 'Тэги' ? 'row' : 'column'}`}>
            {values.map((item) => (
              <label key={item.id ?? item} className="filter-option">
                <input
                  type="checkbox"
                  checked={selected[category].has(item.id ?? item)}
                  onChange={() =>
                    toggleValue(category, item.id ?? item)
                  }
                />
                <span className="filter-text">
                  {item.name ?? '★'.repeat(item)}
                </span>
              </label>
            ))}
          </div>
        </div>
      ))}
    </aside>
  );
};

export default Filter;
