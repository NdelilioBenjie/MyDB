<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <title>Users</title>
    <style>
        body {
            font-family: system-ui, sans-serif;
            background: #f5f7fa;
            padding: 40px;
        }

        h1 {
            color: #111827;
            margin-bottom: 20px;
        }

        a.button {
            display: inline-block;
            margin-bottom: 20px;
            padding: 10px 20px;
            background-color: #2563eb;
            color: white;
            text-decoration: none;
            border-radius: 6px;
            transition: background 0.2s;
        }

        a.button:hover {
            background-color: #1e40af;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);
        }

        th,
        td {
            padding: 14px;
            border-bottom: 1px solid #e5e7eb;
            text-align: left;
        }

        th {
            background: #111827;
            color: white;
        }

        tr:hover {
            background: #f1f5f9;
        }

        .actions {
            display: flex;
            gap: 8px;
        }

        .actions form button,
        .actions a {
            padding: 6px 12px;
            border-radius: 4px;
            border: none;
            cursor: pointer;
            font-size: 14px;
            text-decoration: none;
            color: white;
        }

        .actions a {
            background-color: #10b981;
            /* green for edit */
        }

        .actions a:hover {
            background-color: #047857;
        }

        .actions form button {
            background-color: #ef4444;
            /* red for delete */
        }

        .actions form button:hover {
            background-color: #b91c1c;
        }
    </style>
</head>

<body>

    <h1>Users</h1>
    <a href="/users/create" class="button">+ Add User</a>

    <table>
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Actions</th> <!-- New column -->
            </tr>
        </thead>
        <tbody>
            @forelse ($users as $user)
                <tr>
                    <td>{{ $user['id'] ?? '' }}</td>
                    <td>{{ $user['name'] ?? '' }}</td>
                    <td>{{ $user['email'] ?? '' }}</td>
                    <td>{{ $user['phone'] ?? '' }}</td>
                    <td class="actions">
                        <!-- Edit link -->
                        <a href="/users/{{ $user['id'] }}/edit">Edit</a>

                        <!-- Delete form -->
                        <form action="/users/{{ $user['id'] }}" method="POST"
                            onsubmit="return confirm('Are you sure?');">
                            @csrf
                            @method('DELETE')
                            <button type="submit">Delete</button>
                        </form>
                    </td>
                </tr>
            @empty
                <tr>
                    <td colspan="5">No users found</td>
                </tr>
            @endforelse
        </tbody>
    </table>

</body>

</html>
