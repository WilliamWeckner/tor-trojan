<table>
    <thead>
        <tr>
            <th>ID</th>
            <th>Address</th>
            <th>Added</th>
            <th>Last action</th>
        </tr>
    </thead>
    <tbody>
<? foreach(Model_Client::find('all') as $client):?>
        <tr>
            <td><?=$client->id?></td>
            <td><?=$client->address?>.onion</td>
            <td><?=date('d.m.Y H:i:s', $client->created_at)?></td>
            <td><? //date('d.m.Y H:i:s', $client->updated_at)?></td>
        </tr>
    </tbody>
<? endforeach ?>
</table>