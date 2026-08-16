using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;
using Mafi.Localization;

namespace RecursiveIndustry;

public sealed class FocusWithIconUnlock : IProtoUnlock, IUnlockUnitWithTitleAndIcon
{
    private readonly OfficeFocusProto _focus;
    private readonly ProtoUnlock _unlock;

    public ImmutableArray<IProto> UnlockedProtos => _unlock.UnlockedProtos;

    public bool HideInUI => _unlock.HideInUI;

    public LocStrFormatted Title => _focus.Strings.Name.AsFormatted;

    public LocStrFormatted Description => _focus.Strings.DescShort.AsFormatted;

    public Option<string> IconPath => _focus.IconPath;

    public string FocusId => _focus.Id.Value;

    public FocusWithIconUnlock(OfficeFocusProto focus)
    {
        _focus = focus;
        _unlock = new ProtoUnlock(focus);
    }

    public bool MatchesSearchQuery(string[] query) =>
        _unlock.MatchesSearchQuery(query);
}

[GlobalDependency(RegistrationMode.AsAllInterfaces)]
public sealed class FocusWithIconUnlocker
    : UnitUnlockerBase<FocusWithIconUnlock>
{
    private readonly UnlockedProtosDb _unlockedProtosDb;

    public FocusWithIconUnlocker(UnlockedProtosDb unlockedProtosDb)
    {
        _unlockedProtosDb = unlockedProtosDb;
    }

    public override void Unlock(IIndexable<FocusWithIconUnlock> units)
    {
        IndexableEnumerator<FocusWithIconUnlock> enumerator =
            units.GetEnumerator();
        while (enumerator.MoveNext())
        {
            FocusWithIconUnlock unit = enumerator.Current;
            _unlockedProtosDb.Unlock(unit.UnlockedProtos);
            Log.Info($"RecursiveIndustry: Focus research unlock applied: {unit.FocusId}");
        }
    }
}

internal static class FocusResearchUnlockExtensions
{
    public static ResearchNodeProtoBuilder.State AddFocusToUnlock(
        this ResearchNodeProtoBuilder.State state,
        OfficeFocusProto focus)
    {
        state.AddUnit(new FocusWithIconUnlock(focus));
        state.AddIcon(focus.IconPath);
        return state;
    }
}