import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import ErrorBoundary from './components/ErrorBoundary'
import Login from './pages/Login'
import Callback from './pages/Callback'
import Dashboard from './pages/dashboard/Dashboard'
import DragonBallPage from './pages/dragonball/DragonBallPage'
import MusicPage from './pages/music/MusicPage'

function App() {
  return (
    <BrowserRouter>
      <Layout>
        <ErrorBoundary>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/callback" element={<Callback />} />
            <Route path="/dragonball/*" element={<DragonBallPage />} />
            <Route path="/music/*" element={<MusicPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </ErrorBoundary>
      </Layout>
    </BrowserRouter>
  )
}

export default App
