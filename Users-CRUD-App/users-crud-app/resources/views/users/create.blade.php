<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <title>Create User</title>
    <style>
        body {
            font-family: system-ui, sans-serif;
            background: #f5f7fa;
            padding: 40px;
        }

        form {
            background: white;
            max-width: 400px;
            padding: 30px;
            border-radius: 10px;
        }

        label {
            display: block;
            margin-top: 15px;
            font-weight: 600;
        }

        input {
            width: 100%;
            padding: 10px;
            margin-top: 6px;
            border-radius: 6px;
            border: 1px solid #d1d5db;
        }

        button {
            margin-top: 20px;
            padding: 12px;
            width: 100%;
            background: #111827;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
        }

        .error {
            color: #b91c1c;
            margin-top: 10px;
        }
    </style>
</head>

<body>

    <h1>Create User</h1>

    <form method="POST" action="/users">
        @csrf

        <label>ID</label>
        <input type="number" name="id" value="{{ old('id') }}">

        <label>Name</label>
        <input type="text" name="name" value="{{ old('name') }}">

        <label>Email</label>
        <input type="email" name="email" value="{{ old('email') }}">

        <label>Phone</label>
        <input type="text" name="phone" value="{{ old('phone') }}">

        @error('api')
            <div class="error">{{ $message }}</div>
        @enderror

        <button type="submit">Create</button>
    </form>

</body>

</html>
