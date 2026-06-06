import { BASE_URL } from '@/app/services/http';

export function getImageUrl(path: string | undefined): string | undefined {
  if (!path) return undefined;
  
  // Se já for uma URL completa (http/https), retorna como está
  if (path.startsWith('http')) return path;

  // Remove o sufixo '/api' do BASE_URL para obter o domínio base do servidor
  const backendOrigin = BASE_URL.replace(/\/api$/, '');
  
  // Garante que o path comece com /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  
  return `${backendOrigin}${normalizedPath}`;
}
