export default function TestPage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">StreamVault</h1>
        <p className="text-gray-600 mb-8">shadcn/ui Frontend is Working!</p>
        <div className="space-y-4">
          <a
            href="http://localhost:5000/api/v1/videos"
            className="block text-blue-600 underline"
            target="_blank"
          >
            Test Backend API
          </a>
          <a
            href="/login"
            className="block px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 inline-block"
          >
            Go to Login
          </a>
        </div>
      </div>
    </div>
  );
}
