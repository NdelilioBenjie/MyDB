<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <title>Edit User</title>
    <style>
        body {
            font-family: system-ui, sans-serif;
            background: #f5f7fa;
            padding: 40px;
        }

        form {
            max-width: 500px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        }

        h1 {
            text-align: center;
            margin-bottom: 20px;
            color: #111827;
        }

        label {
            display: block;
            margin-bottom: 6px;
            font-weight: bold;
            color: #374151;
        }

        input[type="text"],
        input[type="email"] {
            width: 100%;
            padding: 10px;
            margin-bottom: 16px;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            box-sizing: border-box;
        }

        button {
            width: 100%;
            padding: 12px;
            background-color: #111827;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            cursor: pointer;
            transition: 0.3s;
        }

        button:hover {
            background-color: #374151;
        }

        a {
            display: inline-block;
            margin-top: 10px;
            color: #2563eb;
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }
    </style>
</head>

<body>
    <form action="/users/{{ $user['id'] }}" method="POST">
        @csrf
        @method('PUT')

        <h1>Edit User</h1>

        <label for="name">Name</label>
        <input type="text" name="name" id="name" value="{{ $user['name'] ?? '' }}" required>

        <label for="email">Email</label>
        <input type="email" name="email" id="email" value="{{ $user['email'] ?? '' }}" required>

        <label for="phone">Phone</label>
        <input type="text" name="phone" id="phone" value="{{ $user['phone'] ?? '' }}" required>

        <button type="submit">Update User</button>

        <a href="/users">← Back to Users</a>
    </form>
</body>

</html>
