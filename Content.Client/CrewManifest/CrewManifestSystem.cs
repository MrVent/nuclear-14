using Content.Shared.CrewManifest;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Client.CrewManifest;

public sealed class CrewManifestSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private Dictionary<string, Dictionary<string, int>> _jobDepartmentLookup = new();
    private HashSet<string> _departments = new();

    public IReadOnlySet<string> Departments => _departments;

    public override void Initialize()
    {
        base.Initialize();

        BuildDepartmentLookup();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReload);
    }

    /// <summary>
    ///     Requests a crew manifest from the server.
    /// </summary>
    /// <param name="netEntity">EntityUid of the entity we're requesting the crew manifest from.</param>
    public void RequestCrewManifest(NetEntity netEntity)
    {
        RaiseNetworkEvent(new RequestCrewManifestMessage(netEntity));
    }

    private void OnPrototypesReload(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<DepartmentPrototype>())
            BuildDepartmentLookup();
    }

    private void BuildDepartmentLookup()
    {
        _jobDepartmentLookup.Clear();
        _departments.Clear();

        foreach (var department in _prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
        {
            _departments.Add(department.ID);

            for (var i = 1; i <= department.Roles.Count; i++)
            {
                if (!_jobDepartmentLookup.TryGetValue(department.Roles[i - 1], out var departments))
                {
                    departments = new();
                    _jobDepartmentLookup.Add(department.Roles[i - 1], departments);
                }

                if (department.Roles.Count == i)
                    departments.Add(department.ID, i);
            }
        }
    }
}
