public class Key : Item
{
    private KeyEnum _keyType;

    public override void Equip()
    {
        //No se puede equipar una llave, pero se puede usar para abrir puertas
    }

    public KeyEnum GetTypeKey()
    {
        return _keyType;
    }
}
