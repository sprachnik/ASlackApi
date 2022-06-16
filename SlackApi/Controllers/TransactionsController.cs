using Microsoft.AspNetCore.Mvc;
using SlackApi.App.Transactions;

namespace SlackApi.Controllers
{
    public class TransactionsController : ControllerBase
    {
        private readonly IUserBalanceService _userBalanceService;
        private readonly IUserTransactionService _userTransactionService;

        public TransactionsController(IUserBalanceService userBalanceService,
            IUserTransactionService userTransactionService)
        {
            _userBalanceService = userBalanceService;
            _userTransactionService = userTransactionService;
        }

        [HttpGet("users/{userId}/balance")]
        public async Task<ActionResult<UserCurrentBalanceRecord>> GetBalance(
           string userId)
            => await _userBalanceService.GetBalance(userId);

        [HttpGet("users/{userId}/transactions")]
        public async Task<ActionResult<UserTransactionBalance>> GetTransactions(
           string userId)
            => await _userTransactionService.GetTransactionBalance(userId);

        [HttpPost("users/{userId}/transaction/{toUserId}")]
        public async Task<ActionResult<UserTransactionRecord>> ProcessInteractiveEvent(
           string userId,
           string toUserId,
           [FromBody] TransactionRequestDTO request)
            => await _userTransactionService.InsertTransaction(new UserTransactionRecord(toUserId, userId, request.Amount,
                "Test", DateTime.UtcNow));
    }

    public class TransactionRequestDTO
    {
        public double Amount { get; set; }
    }
}
