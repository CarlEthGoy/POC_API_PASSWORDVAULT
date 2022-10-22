﻿using API.Database;
using API.Models.V1;

namespace API.BLL
{
  public interface IBLLPassword
  {
    Task<IPasswordModel> GetPasswordById(int password_id);
    Task<int> CreatePassword(IPasswordViewModel passwordToCreate);
    Task<bool> DeletePasswordById(int password_id);
  }

  public class BLLPassword : IBLLPassword
  {
    private readonly IPasswordRepository _passwordRepository;
    private readonly IVaultRepository _vaultRepository;
    private readonly IUserRepository _userRepository;

    public BLLPassword(IPasswordRepository passwordRepository, IVaultRepository vaultRepository, IUserRepository userRepository)
    {
      _passwordRepository = passwordRepository;
      _vaultRepository = vaultRepository;
      _userRepository = userRepository;
    }

    public async Task<IPasswordModel> GetPasswordById(int password_id)
    {
      var password = await _passwordRepository.GetPasswordById(password_id);
      return password;
    }

    public async Task<int> CreatePassword(IPasswordViewModel passwordViewModelToCreate)
    {
      if (string.IsNullOrWhiteSpace(passwordViewModelToCreate.Application_name))
      {
        throw new Exception("Application_name is required.");
      }

      // Valider que le vault existe!
      if (await _vaultRepository.GetVaultById(passwordViewModelToCreate.Vault_id) == null)
      {
        throw new Exception($"The vault with id:{passwordViewModelToCreate.Vault_id} doesn't exist!");
      }

      IPasswordModel passwordModelToCreate = _passwordRepository.GeneratePasswordModelFromPasswordViewModel(passwordViewModelToCreate);
      int createdPasswordId = await _passwordRepository.CreatePassword(passwordModelToCreate);

      bool createdRelationShip = await _passwordRepository.CreateRelationshipMember(passwordViewModelToCreate.Vault_id, createdPasswordId);
      if (!createdRelationShip)
      {
        throw new Exception($"Couln't create the relationship between vault:{passwordViewModelToCreate.Vault_id} and password:{createdPasswordId} !");
      }

      return createdPasswordId;
    }

    public async Task<bool> DeletePasswordById(int password_id)
    {
      return await _passwordRepository.DeletePasswordById(password_id);
    }
  }
}
