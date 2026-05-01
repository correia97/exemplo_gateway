import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import ErrorBoundary from './components/ErrorBoundary'
import Login from './pages/Login'
import Callback from './pages/Callback'
import Dashboard from './pages/dashboard/Dashboard'
import DragonBallPage from './pages/dragonball/DragonBallPage'
import MusicPage from './pages/music/MusicPage'
import AuthGuard from './auth/AuthGuard'
import AdminLayout from './pages/admin/AdminLayout'
import DashboardPage from './pages/admin/DashboardPage'
import CharactersPage from './pages/admin/CharactersPage'
import GenresPage from './pages/admin/GenresPage'
import ArtistsPage from './pages/admin/ArtistsPage'
import AlbumsPage from './pages/admin/AlbumsPage'
import TracksPage from './pages/admin/TracksPage'

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
            <Route path="/admin" element={
              <AuthGuard role="editor" fallback={<Navigate to="/" replace />}>
                <AdminLayout />
              </AuthGuard>
            }>
              <Route index element={<DashboardPage />} />
              <Route path="characters" element={<CharactersPage />} />
              <Route path="genres" element={<GenresPage />} />
              <Route path="artists" element={<ArtistsPage />} />
              <Route path="albums" element={<AlbumsPage />} />
              <Route path="tracks" element={<TracksPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </ErrorBoundary>
      </Layout>
    </BrowserRouter>
  )
}

export default App
