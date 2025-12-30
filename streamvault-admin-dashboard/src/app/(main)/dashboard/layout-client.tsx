"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AppSidebar } from "@/app/(main)/dashboard/_components/sidebar/app-sidebar";
import { Separator } from "@/components/ui/separator";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { SIDEBAR_COLLAPSIBLE_VALUES, SIDEBAR_VARIANT_VALUES } from "@/lib/preferences/layout";
import { cn } from "@/lib/utils";
import { AccountSwitcher } from "./_components/sidebar/account-switcher";
import { LayoutControls } from "./_components/sidebar/layout-controls";
import { SearchDialog } from "./_components/sidebar/search-dialog";
import { ThemeSwitcher } from "./_components/sidebar/theme-switcher";
import { streamvaultApi } from "@/lib/streamvault-api";

interface DashboardLayoutClientProps {
  children: React.ReactNode;
}

export default function DashboardLayoutClient({ children }: DashboardLayoutClientProps) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<any>(null);
  const [variant, setVariant] = useState("inset");
  const [collapsible, setCollapsible] = useState("icon");
  const [defaultOpen, setDefaultOpen] = useState(true);

  useEffect(() => {
    const initDashboard = async () => {
      // Check if user has a token
      const token = localStorage.getItem("auth-token");
      
      if (!token) {
        router.push("/auth/login");
        return;
      }

      try {
        // Fetch user profile
        const profile = await streamvaultApi.user.profile();
        const userData = {
          id: profile.id,
          name: `${profile.firstName ?? ""} ${profile.lastName ?? ""}`.trim() || profile.username || "Admin",
          username: profile.username || (profile.email?.split("@")[0] ?? "admin"),
          email: profile.email,
          avatar: profile.avatar || "",
          role: profile.role || (profile.isSuperAdmin ? "super-admin" : "admin"),
        };
        
        setUser(userData);
        
        // Get preferences from localStorage or defaults
        const savedVariant = localStorage.getItem("sidebar_variant") || "inset";
        const savedCollapsible = localStorage.getItem("sidebar_collapsible") || "icon";
        const savedOpen = localStorage.getItem("sidebar_state") !== "false";
        
        setVariant(savedVariant);
        setCollapsible(savedCollapsible);
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
  }, [router]);

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
