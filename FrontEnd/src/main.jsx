import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
// import App from './App.jsx'
import App1 from './components/WebCrawlerSources'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App1 />
  </StrictMode>,
)
