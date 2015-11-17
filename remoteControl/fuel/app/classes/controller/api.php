<?php

class Controller_Api extends Controller
{

    public function action_add($address='')
    {
        $tmp = explode('.', $address);
        if(count($tmp) == 2 && $tmp[1] == 'onion') return;
        $address = $tmp[0];

        $client = new Model_Client();
        $client->address = $address;
        $client->save();
        return new Response(null, 404);
    }
}