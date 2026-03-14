import { useState, useEffect } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

function App() {
  const [count, setCount] = useState(0)
  const [apiData, setApiData] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const API_BASE_URL = 'http://localhost:5262'; 

    const testApi = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE_URL}/api/Moto/listar`);
        
        if (!response.ok) {
          throw new Error(`Erro na API: ${response.status}`);
        }
        
        const data = await response.text();
        setApiData(data ? 'Conexão com a API bem-sucedida!' : 'Conectado, mas sem dados retornado.');
      } catch (err) {
        setError(err.message);
        console.error('Erro ao conectar na API:', err);
      } finally {
        setLoading(false);
      }
    };

    testApi();
  }, []);

  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      
      <div className="card" style={{ backgroundColor: '#2a2a2a', margin: '20px 0', padding: '20px', borderRadius: '8px' }}>
        <h2>Status da API MotoRev</h2>
        {loading && <p>Testando conexão com a API...</p>}
        {error && <p style={{ color: '#ff6b6b' }}>Falha na conexão: {error}</p>}
        {apiData && <p style={{ color: '#51cf66' }}>{apiData}</p>}
      </div>

      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.jsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  )
}

export default App
