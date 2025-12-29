import {
  LayoutDashboard,
  type LucideIcon,
  Upload,
  Users,
  Video,
  SquareArrowUpRight,
} from "lucide-react";

export interface NavSubItem {
  title: string;
  url: string;
  icon?: LucideIcon;
  comingSoon?: boolean;
  newTab?: boolean;
  isNew?: boolean;
}

export interface NavMainItem {
  title: string;
  url: string;
  icon?: LucideIcon;
  subItems?: NavSubItem[];
  comingSoon?: boolean;
  newTab?: boolean;
  isNew?: boolean;
}

export interface NavGroup {
  id: number;
  label?: string;
  items: NavMainItem[];
}

export const sidebarItems: NavGroup[] = [
  {
    id: 1,
    label: "StreamVault",
    items: [
      {
        title: "Dashboard",
        url: "/dashboard",
        icon: LayoutDashboard,
      },
      {
        title: "Videos",
        url: "/dashboard/videos",
        icon: Video,
      },
      {
        title: "Upload",
        url: "/dashboard/upload",
        icon: Upload,
      },
      {
        title: "Users",
        url: "/dashboard/users",
        icon: Users,
      },
    ],
  },
  {
    id: 2,
    label: "Admin",
    items: [
      {
        title: "Settings",
        url: "/dashboard/settings",
        comingSoon: true,
      },
    ],
  },
  {
    id: 3,
    label: "Misc",
    items: [
      {
        title: "Docs",
        url: "/dashboard/coming-soon",
        icon: SquareArrowUpRight,
        comingSoon: true,
      },
    ],
  },
];
