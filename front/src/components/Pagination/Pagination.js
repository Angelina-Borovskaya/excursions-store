import React from "react";
import "./Pagination.css";

export const Pagination = ({ currentPage, totalPages, onPageChange }) => {
  const mk = (text, disabled, page) => (
    <button
      className={`page-btn ${currentPage === page ? "active" : ""}`}
      disabled={disabled}
      onClick={() => onPageChange(page)}
      key={text}
    >
      {text}
    </button>
  );

  const windowSize = 2;
  const start = Math.max(1, currentPage - windowSize);
  const end = Math.min(totalPages, currentPage + windowSize);
  const buttons = [];

  buttons.push(mk("«", currentPage === 1, 1));
  buttons.push(mk("‹", currentPage === 1, currentPage - 1));

  for (let p = start; p <= end; p++) buttons.push(mk(p, false, p));

  buttons.push(mk("›", currentPage === totalPages, currentPage + 1));
  buttons.push(mk("»", currentPage === totalPages, totalPages));

  return <div className="pagination">{buttons}</div>;
};
