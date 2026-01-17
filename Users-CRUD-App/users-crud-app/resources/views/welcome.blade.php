<!DOCTYPE html>

<html lang="en">

<head>
    <meta charset="UTF-8">
    <title>MyDB Users App</title>
    <style>
        body {
            font-family: system-ui, sans-serif;
            background: #f5f7fa;
            height: 100vh;
            margin: 0;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        ``` .card {
            background: white;
            padding: 40px 50px;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
            text-align: center;
            max-width: 420px;
            width: 100%;
        }

        h1 {
            margin-bottom: 10px;
            color: #111827;
        }

        p {
            color: #6b7280;
            margin-bottom: 30px;
            font-size: 15px;
        }

        .actions {
            display: flex;
            gap: 15px;
            justify-content: center;
        }

        a.button {
            display: inline-block;
            padding: 12px 22px;
            background-color: #111827;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 500;
            transition: 0.3s;
        }

        a.button.secondary {
            background-color: #2563eb;
        }

        a.button:hover {
            opacity: 0.9;
            transform: translateY(-1px);
        }
    </style>
    ```

</head>

<body>

    ```
    <div class="card">
        <h1>MyDB Users App</h1>
        <p>
            A simple Laravel client powered by a custom-built database engine.
        </p>

        <div class="actions">
            <a href="/users" class="button">View Users</a>
            <a href="/users/create" class="button secondary">Add User</a>
        </div>
    </div>
    ```

</body>

</html>
