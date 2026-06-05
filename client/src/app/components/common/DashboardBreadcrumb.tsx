import { Breadcrumb } from 'antd';
import { HomeOutlined, CarOutlined, ToolOutlined, UserOutlined } from '@ant-design/icons';
import { Link } from 'react-router';

interface BreadcrumbItem {
  title: string | React.ReactNode;
  path?: string;
  icon?: React.ReactNode;
}

interface DashboardBreadcrumbProps {
  userType: 'cliente' | 'concessionaria';
  items: BreadcrumbItem[];
}

export default function DashboardBreadcrumb({ userType, items }: DashboardBreadcrumbProps) {
  const basePath = `/dashboard/${userType}`;

  const breadcrumbItems = [
    {
      title: <Link to={basePath} aria-label="Home"><HomeOutlined /></Link>,
    },
    ...items.map((item, index) => {
      const isLast = index === items.length - 1;

      if (item.path && !isLast) {
        const fullPath = item.path.startsWith('/') ? item.path : `${basePath}${item.path}`;
        
        return {
          title: item.icon ? (
            <Link to={fullPath}>
              {item.icon}
              <span style={{ marginLeft: '4px' }}>{item.title}</span>
            </Link>
          ) : (
            <Link to={fullPath}>{item.title}</Link>
          ),
        };
      }

      return {
        title: item.icon ? (
          <>
            {item.icon}
            <span style={{ marginLeft: '4px' }}>{item.title}</span>
          </>
        ) : item.title,
      };
    }),
  ];

  return <Breadcrumb items={breadcrumbItems} />;
}