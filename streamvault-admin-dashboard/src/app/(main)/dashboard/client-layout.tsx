"use client";

import DashboardWrapper from "./dashboard-wrapper";

interface DashboardLayoutProps {
  children: React.ReactNode;
}

export default function DashboardLayout({ children }: DashboardLayoutProps) {
  return <DashboardWrapper>{children}</DashboardWrapper>;
}
