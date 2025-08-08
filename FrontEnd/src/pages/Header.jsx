import React from 'react';
import { Button } from 'antd';
import { useNavigate } from 'react-router-dom';

const Header = () => {
  const navigate = useNavigate();

  return (
    <div style={{ padding: '16px', display: 'flex', gap: '16px'}}>
      <Button type="primary" onClick={() => navigate('/')}>
        Trang nguồn crawl
      </Button>
      <Button type="default" onClick={() => navigate('/tours')}>
        Danh sách tour đã crawl
      </Button>
    </div>
  );
};

export default Header;
