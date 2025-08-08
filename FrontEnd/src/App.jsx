// App.jsx
import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout } from 'antd';
import TourTable from './components/TourTable';
import WebCrawlerSources from './components/WebCrawlerSources';
import HeaderBar from './pages/header';

const { Header, Content } = Layout;

function App() {
  return (
    <Router>
      <Layout style={{ minHeight: '100vh' }}>
        <Header style={{ background: '#fff'}}>
          <HeaderBar />
        </Header>
        <Content style={{ margin: '24px' }}>
          <Routes>
            <Route path="/" element={<WebCrawlerSources />} />
            <Route path="/tours" element={<TourTable />} />
          </Routes>
        </Content>
      </Layout>
    </Router>
  );
}

export default App;
