<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class UserController extends Controller
{
    protected $apiUrl;

    public function __construct()
    {
        $this->apiUrl = config('services.mydb.url');
    }

    // List all users
    public function index()
    {
        $response = Http::get("{$this->apiUrl}/users");

        if (!$response->successful()) {
            abort(500, 'Failed to fetch users');
        }

        return view('users.index', [
            'users' => $response->json()
        ]);
    }

    // Show form to create a new user
    public function create()
    {
        return view('users.create');
    }

    // Store new user via API
    public function store(Request $request)
    {
        $validated = $request->validate([
            'id'    => 'required|integer',
            'name'  => 'required|string',
            'email' => 'required|email',
            'phone' => 'required|string',
        ]);

        $response = Http::post("{$this->apiUrl}/users", $validated);

        if (!$response->successful()) {
            return back()->withErrors([
                'api' => $response->json('detail') ?? 'Insert failed'
            ])->withInput();
        }

        return redirect('/users')->with('success', 'User created');
    }

    // Show form to edit an existing user
    public function edit($id)
    {
        $response = Http::get("{$this->apiUrl}/users/{$id}");

        if (!$response->successful()) {
            abort(404, 'User not found');
        }

        $user = $response->json();
        return view('users.edit', ['user' => $user]);
    }

    // Update user via API
    public function update(Request $request, $id)
    {
        $validated = $request->validate([
            'name'  => 'required|string',
            'email' => 'required|email',
            'phone' => 'required|string',
        ]);

        $response = Http::put("{$this->apiUrl}/users/{$id}", $validated);

        if (!$response->successful()) {
            return back()->withErrors([
                'api' => $response->json('detail') ?? 'Update failed'
            ])->withInput();
        }

        return redirect('/users')->with('success', 'User updated');
    }

    // Delete user via API
    public function destroy($id)
    {
        $response = Http::delete("{$this->apiUrl}/users/{$id}");

        if (!$response->successful()) {
            return back()->withErrors([
                'api' => $response->json('detail') ?? 'Delete failed'
            ]);
        }

        return redirect('/users')->with('success', 'User deleted');
    }
}
