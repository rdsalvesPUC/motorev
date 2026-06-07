import { useState } from 'react';
import { Layout, Menu, Button, Typography, Dropdown, Avatar, message } from 'antd';
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  CarOutlined,
  CalendarOutlined,
  ToolOutlined,
  ShopOutlined,
  DashboardOutlined,
  HomeOutlined,
  BookOutlined,
  UserOutlined,
  LogoutOutlined,
  SettingOutlined,
  BulbOutlined,
  BulbFilled,
  GlobalOutlined,
} from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { useNavigate, useLocation } from 'react-router';
import { PATHS } from '@/app/paths';
import { authService } from '@/app/services/authService';
import { t } from '@/app/i18n';
import { useConfiguracoes } from '@/app/contexts/ConfiguracoesContext';

const { Sider, Content } = Layout;
const { Text } = Typography;

function Logo() {
  return (
    <svg width="112" height="18" viewBox="0 0 112 18" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M97.4723 4.752H103.408L104.556 12.984L108.644 4.752H111.612L105.256 16.792H99.4043L97.4723 4.752Z" fill="#1B2128"/>
      <path d="M87.4465 11.164C87.3718 11.5747 87.3158 11.9667 87.2785 12.34C87.2598 12.7133 87.2505 13.0493 87.2505 13.348C87.2505 13.516 87.2598 13.7027 87.2785 13.908C87.2972 14.1133 87.3345 14.3093 87.3905 14.496C87.4652 14.664 87.5585 14.8133 87.6705 14.944C87.8012 15.056 87.9785 15.112 88.2025 15.112C88.3705 15.112 88.5572 15.056 88.7625 14.944C88.9865 14.832 89.2012 14.6733 89.4065 14.468C89.6305 14.2627 89.8358 14.0107 90.0225 13.712C90.2278 13.4133 90.3865 13.0773 90.4985 12.704H95.0905L94.3625 15.7C93.9892 15.9427 93.5598 16.148 93.0745 16.316C92.5892 16.484 92.0758 16.624 91.5345 16.736C90.9932 16.8293 90.4425 16.8947 89.8825 16.932C89.3225 16.988 88.7905 17.016 88.2865 17.016C87.3532 17.016 86.4852 16.932 85.6825 16.764C84.8798 16.596 84.1798 16.3067 83.5825 15.896C82.9852 15.4853 82.5092 14.944 82.1545 14.272C81.8185 13.5813 81.6505 12.732 81.6505 11.724C81.6505 10.8093 81.7905 9.91333 82.0705 9.036C82.3692 8.15867 82.8545 7.384 83.5265 6.712C84.2172 6.02133 85.1318 5.47067 86.2705 5.06C87.4278 4.64933 88.8652 4.444 90.5825 4.444C92.4492 4.444 93.8492 4.73333 94.7825 5.312C95.7345 5.89067 96.2105 6.796 96.2105 8.028C96.2105 8.532 96.1358 9.08267 95.9865 9.68C95.8372 10.2587 95.6132 10.7533 95.3145 11.164H87.4465ZM91.1705 7.832C91.1705 7.25333 91.0772 6.86133 90.8905 6.656C90.7038 6.45067 90.4798 6.348 90.2185 6.348C89.9198 6.348 89.6398 6.45067 89.3785 6.656C89.1358 6.84267 88.9118 7.104 88.7065 7.44C88.5012 7.75733 88.3145 8.13067 88.1465 8.56C87.9785 8.97067 87.8385 9.40933 87.7265 9.876H90.9185C90.9558 9.652 90.9932 9.41867 91.0305 9.176C91.0678 8.97067 91.0958 8.74667 91.1145 8.504C91.1518 8.26133 91.1705 8.03733 91.1705 7.832Z" fill="#1B2128"/>
      <path d="M70.0753 4.752H74.6393L75.0593 6.852H75.2553C75.6847 6.18 76.2353 5.61067 76.9073 5.144C77.5793 4.67733 78.466 4.444 79.5673 4.444C79.642 4.444 79.754 4.45333 79.9033 4.472C80.0713 4.472 80.2487 4.5 80.4353 4.556C80.6407 4.59333 80.846 4.65867 81.0513 4.752C81.2753 4.82667 81.49 4.93867 81.6953 5.088L80.4633 10.856H77.4113C77.374 9.69867 77.2713 8.896 77.1033 8.448C76.9353 7.98133 76.6647 7.748 76.2913 7.748C76.1047 7.748 75.8993 7.79467 75.6753 7.888C75.47 7.96267 75.2553 8.14 75.0313 8.42L73.2393 16.792H67.5273L70.0753 4.752Z" fill="#1B2128"/>
      <path d="M60.5219 4.444C62.7432 4.444 64.3579 4.836 65.3659 5.62C66.4299 6.44133 66.9619 7.804 66.9619 9.708C66.9619 10.884 66.7659 11.948 66.3739 12.9C65.9819 13.8333 65.4312 14.608 64.7219 15.224C63.3032 16.4187 61.2499 17.016 58.5619 17.016C56.2659 17.016 54.6232 16.5773 53.6339 15.7C52.5699 14.7853 52.0379 13.4133 52.0379 11.584C52.0379 10.3707 52.2805 9.26933 52.7659 8.28C53.2699 7.29067 53.9885 6.46933 54.9219 5.816C56.2845 4.90133 58.1512 4.444 60.5219 4.444ZM58.6179 15.112C59.0845 15.112 59.4859 14.8507 59.8219 14.328C60.1765 13.8053 60.4565 13.1707 60.6619 12.424C60.8672 11.6587 61.0165 10.8747 61.1099 10.072C61.2219 9.25067 61.2779 8.54133 61.2779 7.944C61.2779 7.42133 61.2219 7.02933 61.1099 6.768C61.0165 6.488 60.8019 6.348 60.4659 6.348C59.9619 6.348 59.5232 6.61867 59.1499 7.16C58.7952 7.70133 58.5059 8.34533 58.2819 9.092C58.0579 9.83867 57.8899 10.604 57.7779 11.388C57.6845 12.172 57.6379 12.8067 57.6379 13.292C57.6379 14.5053 57.9645 15.112 58.6179 15.112Z" fill="#1B2128"/>
      <path d="M42.2711 4.752C42.9618 4.60267 43.5685 4.43467 44.0911 4.248C44.6138 4.06133 45.0898 3.828 45.5191 3.548C45.9671 3.268 46.3871 2.92267 46.7791 2.512C47.1898 2.10133 47.6285 1.59733 48.0951 1H50.9791L50.1671 4.752H52.3511L51.9871 6.628H49.7751L48.6271 12.088C48.5338 12.5173 48.4591 12.9 48.4031 13.236C48.3471 13.572 48.3191 13.8333 48.3191 14.02C48.3191 14.3933 48.4498 14.6267 48.7111 14.72C48.9911 14.8133 49.5045 14.86 50.2511 14.86L49.8311 16.792C49.7005 16.8293 49.4951 16.8573 49.2151 16.876C48.9538 16.8947 48.6551 16.9133 48.3191 16.932C48.0018 16.9693 47.6658 16.988 47.3111 16.988C46.9751 17.0067 46.6765 17.016 46.4151 17.016C45.9485 17.016 45.4911 16.9787 45.0431 16.904C44.5951 16.848 44.1938 16.7267 43.8391 16.54C43.4845 16.3347 43.1951 16.0453 42.9711 15.672C42.7658 15.2987 42.6631 14.7947 42.6631 14.16C42.6631 13.936 42.6725 13.656 42.6911 13.32C42.7285 12.984 42.7938 12.6107 42.8871 12.2L44.0631 6.628H41.8791L42.2711 4.752Z" fill="#1B2128"/>
      <path d="M34.2172 4.444C36.4385 4.444 38.0532 4.836 39.0612 5.62C40.1252 6.44133 40.6572 7.804 40.6572 9.708C40.6572 10.884 40.4612 11.948 40.0692 12.9C39.6772 13.8333 39.1265 14.608 38.4172 15.224C36.9985 16.4187 34.9452 17.016 32.2572 17.016C29.9612 17.016 28.3185 16.5773 27.3292 15.7C26.2652 14.7853 25.7332 13.4133 25.7332 11.584C25.7332 10.3707 25.9759 9.26933 26.4612 8.28C26.9652 7.29067 27.6839 6.46933 28.6172 5.816C29.9799 4.90133 31.8465 4.444 34.2172 4.444ZM32.3132 15.112C32.7799 15.112 33.1812 14.8507 33.5172 14.328C33.8718 13.8053 34.1518 13.1707 34.3572 12.424C34.5625 11.6587 34.7119 10.8747 34.8052 10.072C34.9172 9.25067 34.9732 8.54133 34.9732 7.944C34.9732 7.42133 34.9172 7.02933 34.8052 6.768C34.7119 6.488 34.4972 6.348 34.1612 6.348C33.6572 6.348 33.2185 6.61867 32.8452 7.16C32.4905 7.70133 32.2012 8.34533 31.9772 9.092C31.7532 9.83867 31.5852 10.604 31.4732 11.388C31.3799 12.172 31.3332 12.8067 31.3332 13.292C31.3332 14.5053 31.6599 15.112 32.3132 15.112Z" fill="#1B2128"/>
      <path d="M8.784 6.544C9.176 6.17067 9.61467 5.82533 10.1 5.508C10.5293 5.24667 11.0427 5.004 11.64 4.78C12.2373 4.556 12.9093 4.444 13.656 4.444C14.552 4.444 15.2147 4.64933 15.644 5.06C16.092 5.47067 16.344 5.97467 16.4 6.572H16.568C16.9973 6.18 17.464 5.82533 17.968 5.508C18.3973 5.24667 18.9013 5.004 19.48 4.78C20.0587 4.556 20.6747 4.444 21.328 4.444C22.2987 4.444 23.0827 4.71467 23.68 5.256C24.296 5.77867 24.604 6.46 24.604 7.3C24.604 7.72933 24.5293 8.29867 24.38 9.008C24.2307 9.71733 24.0627 10.492 23.876 11.332L22.7 16.792H17.184L18.696 9.484C18.752 9.204 18.808 8.93333 18.864 8.672C18.92 8.41067 18.948 8.196 18.948 8.028C18.948 7.76667 18.8733 7.552 18.724 7.384C18.5747 7.19733 18.36 7.104 18.08 7.104C17.7253 7.104 17.3893 7.23467 17.072 7.496C16.7733 7.73867 16.54 7.944 16.372 8.112L14.552 16.792H9.316L10.856 9.484C10.8933 9.24133 10.94 8.99867 10.996 8.756C11.052 8.49467 11.08 8.26133 11.08 8.056C11.08 7.776 11.0053 7.552 10.856 7.384C10.7253 7.19733 10.52 7.104 10.24 7.104C9.904 7.104 9.57733 7.22533 9.26 7.468C8.96133 7.71067 8.728 7.916 8.56 8.084L6.712 16.792H1L3.548 4.752H8.112L8.616 6.544H8.784Z" fill="#1B2128"/>
    </svg>
  );
}

type UserType = 'cliente' | 'concessionaria';

interface DashboardLayoutProps {
  userType: UserType;
  userName: string;
  children: React.ReactNode;
}

export default function DashboardLayout({ userType, userName, children }: DashboardLayoutProps) {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { configuracoes, setIdioma, toggleTema } = useConfiguracoes();

  const handleLogout = async () => {
    try {
      await authService.logout();
      // O logout já limpa os tokens via tokenManager.clearTokens() no finally
      navigate(PATHS.LOGIN);
    } catch (error: any) {
      message.error(error.message || t('dashboard.logout.error'));
      navigate(PATHS.LOGIN);
    }
  };

  const handleProfile = () => {
    const profilePath = userType === 'cliente' 
      ? `${PATHS.DASHBOARD_CLIENTE}${PATHS.PERFIL_USUARIO}`
      : `${PATHS.DASHBOARD_CONCESSIONARIA}${PATHS.PERFIL_USUARIO}`;
    navigate(profilePath);
  };

  const handleMenuClick = (path: string) => {
    navigate(path);
  };

  const getSelectedKey = () => {
    const { pathname } = location;
    if (pathname === PATHS.DASHBOARD_CONCESSIONARIA) {
      return PATHS.CONCESSIONARIA_DASHBOARD;
    }
    return pathname;
  };

  const clienteMenuItems: MenuProps['items'] = [
    {
      key: PATHS.CLIENTE_MOTOS,
      icon: <CarOutlined />,
      label: t('dashboard.menu.motos'),
    },
    {
      key: PATHS.CLIENTE_AGENDAMENTOS,
      icon: <CalendarOutlined />,
      label: t('dashboard.menu.agendamentos'),
    },
    {
      key: PATHS.CLIENTE_REVISOES,
      icon: <ToolOutlined />,
      label: t('dashboard.menu.revisoes'),
    },
    {
      key: PATHS.CLIENTE_CONCESSIONARIAS,
      icon: <ShopOutlined />,
      label: t('dashboard.menu.concessionarias'),
    },
  ];

  const concessionariaMenuItems: MenuProps['items'] = [
    {
      key: PATHS.CONCESSIONARIA_DASHBOARD,
      icon: <DashboardOutlined />,
      label: t('dashboard.menu.dashboard'),
    },
    {
      key: PATHS.CONCESSIONARIA_LOJAS,
      icon: <HomeOutlined />,
      label: t('dashboard.menu.lojas'),
    },
    {
      key: `${PATHS.DASHBOARD_CONCESSIONARIA}${PATHS.PERFIL_USUARIO}`,
      icon: <UserOutlined />,
      label: t('dashboard.userMenu.myProfile'),
    },
    {
      key: PATHS.CONCESSIONARIA_AGENDAMENTOS,
      icon: <CalendarOutlined />,
      label: t('dashboard.menu.agendamentos'),
    },
    {
      key: PATHS.CONCESSIONARIA_CATALOGOS,
      icon: <BookOutlined />,
      label: t('dashboard.menu.catalogos'),
      children: [
        {
          key: PATHS.CONCESSIONARIA_CATALOGOS_MOTOS,
          label: t('dashboard.menu.catalogosMotos'),
        },
        {
          key: PATHS.CONCESSIONARIA_CATALOGOS_LINHAS,
          label: t('dashboard.menu.catalogosLinhas'),
        },
        {
          key: PATHS.CONCESSIONARIA_CATALOGOS_REVISOES,
          label: t('dashboard.menu.catalogosRevisoes'),
        },
        {
          key: PATHS.CONCESSIONARIA_CATALOGOS_PECAS,
          label: t('dashboard.menu.catalogosPecas'),
        },
        {
          key: PATHS.CONCESSIONARIA_CATALOGOS_SERVICOS,
          label: t('dashboard.menu.catalogosServicos'),
        },
      ],
    },
  ];

  const menuItems = userType === 'cliente' ? clienteMenuItems : concessionariaMenuItems;

  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <SettingOutlined />,
      label: t('dashboard.userMenu.myProfile'),
      onClick: handleProfile,
    },
    {
      type: 'divider',
    },
    {
      key: 'language',
      icon: <GlobalOutlined />,
      label: configuracoes.idioma === 'pt-BR' ? t('dashboard.userMenu.languagePtBr') : t('dashboard.userMenu.languageEnUs'),
      children: [
        {
          key: 'language-pt-BR',
          label: t('dashboard.userMenu.languagePtBrFull'),
          onClick: () => setIdioma('pt-BR'),
        },
        {
          key: 'language-en-US',
          label: t('dashboard.userMenu.languageEnUsFull'),
          onClick: () => setIdioma('en-US'),
        },
      ],
    },
    {
      key: 'theme',
      icon: configuracoes.tema === 'dark' ? <BulbFilled /> : <BulbOutlined />,
      label: configuracoes.tema === 'dark' ? t('dashboard.userMenu.darkTheme') : t('dashboard.userMenu.lightTheme'),
      onClick: toggleTema,
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: t('dashboard.userMenu.logout'),
      onClick: handleLogout,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={collapsed}
        theme="light"
        style={{ position: 'sticky', top: 0, height: '100vh', overflow: 'hidden' }}
      >
        <div style={{ padding: '16px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          {!collapsed && (
            <div style={{ height: '24px' }}>
              <Logo />
            </div>
          )}
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed(!collapsed)}
          />
        </div>

        <Menu
          mode="inline"
          selectedKeys={[getSelectedKey()]}
          items={menuItems}
          style={{ borderRight: 0 }}
          onClick={(e) => handleMenuClick(e.key)}
        />

        <div style={{ position: 'absolute', bottom: 0, width: '100%', padding: '16px', borderTop: '1px solid #f0f0f0' }}>
          <Dropdown menu={{ items: userMenuItems }} placement="topLeft" trigger={['click']}>
            <div style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', overflow: 'hidden' }}>
              <Avatar icon={<UserOutlined />} style={{ flexShrink: 0 }} />
              {!collapsed && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 0, minWidth: 0 }}>
                  <Text strong ellipsis style={{ display: 'block' }}>{userName}</Text>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {userType === 'cliente' ? t('dashboard.userType.client') : t('dashboard.userType.dealership')}
                  </Text>
                </div>
              )}
            </div>
          </Dropdown>
        </div>
      </Sider>

      <Layout>
        <Content style={{ margin: '24px', minHeight: 280 }}>
          {children}
        </Content>
      </Layout>
    </Layout>
  );
}
