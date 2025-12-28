import type { Metadata } from 'next';
import { ReactNode } from 'react';
import { Inter } from 'next/font/google';
import './globals.css';
import { AuthProvider } from '@/contexts/AuthContext';
import { QueryProvider } from '@/providers/QueryProvider';
import { Toaster } from 'sonner';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'StreamVault - Video Streaming Platform',
  description: 'Professional video streaming platform for creators and businesses',
  keywords: 'video streaming, hosting, platform, creator, business',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className="h-full" suppressHydrationWarning>
      <body className={inter.className + " h-full bg-gray-50 antialiased"}>
        <QueryProvider>
          <AuthProvider>
            <div className="min-h-full">
              {children}
            </div>
            <Toaster />
          </AuthProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
