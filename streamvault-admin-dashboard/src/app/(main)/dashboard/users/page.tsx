import { ShieldCheck, User } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { streamvaultApi } from "@/lib/streamvault-api";

export default async function Page() {
  const profile = await streamvaultApi.user.profile();

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-muted-foreground">User management is limited to the endpoints currently implemented</p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="size-5" /> Current Profile
          </CardTitle>
          <CardDescription>Fetched from /api/v1/user/profile</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-2">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="secondary">{profile.email}</Badge>
            {profile.isSuperAdmin ? (
              <Badge>
                <ShieldCheck className="mr-1 size-3" /> Super Admin
              </Badge>
            ) : (
              <Badge variant="outline">Admin</Badge>
            )}
          </div>

          <div className="text-sm text-muted-foreground">
            Name: {(profile.firstName ?? "")} {(profile.lastName ?? "")}
          </div>
          <div className="text-sm text-muted-foreground">Username: {profile.username ?? "admin"}</div>
          <div className="text-sm text-muted-foreground">Role: {profile.role ?? (profile.isSuperAdmin ? "super-admin" : "admin")}</div>
        </CardContent>
      </Card>

      <div className="rounded-lg border bg-card p-4 text-sm text-muted-foreground">
        To enable full user management (list users, roles, tenants), we’ll need corresponding backend endpoints.
      </div>
    </div>
  );
}
