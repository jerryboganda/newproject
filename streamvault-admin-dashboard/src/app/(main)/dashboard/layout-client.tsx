"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AppSidebar } from "@/app/(main)/dashboard/_components/sidebar/app-sidebar";
import { Separator } from "@/components/ui/separator";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { SIDEBAR_COLLAPSIBLE_VALUES, SIDEBAR_VARIANT_VALUES, type SidebarCollapsible, type SidebarVariant } from "@/lib/preferences/layout";
import { cn } from "@/lib/utils";
import { AccountSwitcher } from "./_components/sidebar/account-switcher";
import { LayoutControls } from "./_components/sidebar/layout-controls";
import { SearchDialog } from "./_components/sidebar/search-dialog";
import { ThemeSwitcher } from "./_components/sidebar/theme-switcher";
import { useAuthStore } from "@/stores/auth-store";

interface DashboardLayoutClientProps {
  children: React.ReactNode;
}

export default function DashboardLayoutClient({ children }: DashboardLayoutClientProps) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<any>(null);
  const [variant, setVariant] = useState<SidebarVariant>("inset");
  const [collapsible, setCollapsible] = useState<SidebarCollapsible>("icon");
  const [defaultOpen, setDefaultOpen] = useState(true);
  const authUser = useAuthStore((s) => s.user);

  useEffect(() => {
    const initDashboard = async () => {
      // Check if user has a token
      const token = localStorage.getItem("auth-token");
      
      if (!token) {
        router.push("/auth/login");
        return;
      }

      try {
        // Backend profile endpoint isn't implemented yet.
        // Use the user object returned from login (persisted in auth-store) as the sidebar identity.
        const email = authUser?.email;
        const displayName = `${authUser?.firstName ?? ""} ${authUser?.lastName ?? ""}`.trim();
        const userData = {
          id: authUser?.id ?? "local",
          name: displayName || email || "Admin",
          username: email ? email.split("@")[0] : "admin",
          email: email ?? "",
          avatar: authUser?.avatarUrl ?? "",
          role: authUser?.roles?.includes("SuperAdmin") ? "super-admin" : "admin",
        };
        
        setUser(userData);
        
        // Get preferences from localStorage or defaults
        const savedVariant = localStorage.getItem("sidebar_variant") || "inset";
        const savedCollapsible = localStorage.getItem("sidebar_collapsible") || "icon";
        const savedOpen = localStorage.getItem("sidebar_state") !== "false";

        const allowedVariants = SIDEBAR_VARIANT_VALUES as readonly SidebarVariant[];
        const allowedCollapsible = SIDEBAR_COLLAPSIBLE_VALUES as readonly SidebarCollapsible[];

        setVariant(allowedVariants.includes(savedVariant as SidebarVariant) ? (savedVariant as SidebarVariant) : "inset");
        setCollapsible(
          allowedCollapsible.includes(savedCollapsible as SidebarCollapsible)
            ? (savedCollapsible as SidebarCollapsible)
            : "icon",
        );
        setDefaultOpen(savedOpen);
        
      } catch (error) {
        console.error("Failed to load dashboard:", error);
        localStorage.removeItem("auth-token");
        router.push("/auth/login");
      } finally {
        setIsLoading(false);
      }
    };

    initDashboard();
  }, [router, authUser]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <SidebarProvider defaultOpen={defaultOpen}>
      <AppSidebar user={user} variant={variant} collapsible={collapsible} />
      <SidebarInset
        className={cn(
          "flex-1 overflow-hidden",
          "transition-all duration-300 ease-in-out"
        )}
      >
        <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <SearchDialog />
          <div className="ml-auto flex items-center gap-2">
            <ThemeSwitcher />
            <LayoutControls />
            <AccountSwitcher users={[user]} />
          </div>
        </header>

        <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
          {children}
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
